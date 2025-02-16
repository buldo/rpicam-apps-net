using XenoAtom.Interop;

namespace DrmPreview;

using static Interop.libdrm;
using static XenoAtom.Interop.libdrm;

public class DrmPreview
{
    private readonly int drmfd_;
    private uint conId_;
    private uint max_image_width_;
    private uint max_image_height_;
    private uint screen_width_;
    private uint screen_height_;
    private uint crtcId_;
    private int crtcIdx_;
    private uint x_;
    private uint y_;
    private uint width_;
    private uint height_;

    public DrmPreview()
    {
        drmfd_ = drmOpen("vc4", null);
        if (drmfd_ < 0)
        {
            //throw new Exception("drmOpen failed: " + std::string(ERRSTR));
            throw new Exception("drmOpen failed");
        }

        try
        {
            if (!Interop.libdrm.drmIsMaster(drmfd_))
            {
                throw new Exception("DRM preview unavailable - not master");
            }

            conId_ = 0;
            findCrtc();
            //out_fourcc_ = DRM_FORMAT_YUV420;
            //findPlane();
        }
        catch (Exception e)
        {
            //close(drmfd_);
            throw;
        }

        //x_ = y_ = 0;
        //width_ = screen_width_;
        //height_ = screen_height_;
    }

    private unsafe void findCrtc()
    {
        int i;
        drmModeRes* res = libdrm.drmModeGetResources(drmfd_);
        if (res == null)
        {
            //throw std::runtime_error("drmModeGetResources failed: " + std::string(ERRSTR));
            throw new Exception("drmModeGetResources failed");
        }

        if (res->count_crtcs <= 0)
        {
            drmModeFreeResources(res);
            throw new Exception("drm: no crts");
        }

        max_image_width_ = res->max_width;
        max_image_height_ = res->max_height;

        if (conId_ == 0)
        {
            //LOG(2, "No connector ID specified.  Choosing default from list:");

            for (i = 0; i < res->count_connectors; i++)
            {
                drmModeConnector* con = drmModeGetConnector(drmfd_, res->connectors[i]);
                drmModeEncoder* enc = null;
                drmModeCrtc* crtc = null;

                if (con->encoder_id != 0)
                {
                    enc = drmModeGetEncoder(drmfd_, con->encoder_id);
                    if (enc->crtc_id != 0)
                    {
                        crtc = drmModeGetCrtc(drmfd_, enc->crtc_id);
                    }
                }

                if (conId_ == 0 && crtc != null)
                {
                    conId_ = con->connector_id;
                    crtcId_ = crtc->crtc_id;
                }

                if (crtc != null)
                {
                    screen_width_ = crtc->width;
                    screen_height_ = crtc->height;
                }

                //LOG(2, "Connector " << con->connector_id << " (crtc " << (crtc ? crtc->crtc_id : 0) << "): type "
                //                    << con->connector_type << ", " << (crtc ? crtc->width : 0) << "x"
                //                    << (crtc ? crtc->height : 0) << (conId_ == (int)con->connector_id ? " (chosen)" : ""));

                if (con->encoder_id != 0)
                {
                    drmModeFreeEncoder(enc);
                    if (enc->crtc_id != 0)
                    {
                        drmModeFreeCrtc(crtc);
                    }
                }
                drmModeFreeConnector(con);
            }

            if (conId_ == 0)
            {
                drmModeFreeResources(res);
                throw new Exception("No suitable enabled connector found");
            }
        }

        crtcIdx_ = -1;

        for (i = 0; i < res->count_crtcs; ++i)
        {
            if (crtcId_ == res->crtcs[i])
            {
                crtcIdx_ = i;
                break;
            }
        }

        if (crtcIdx_ == -1)
        {
            drmModeFreeResources(res);
            throw new Exception("drm: CRTC " +crtcId_ + " not found");
        }

        if (res->count_connectors <= 0)
        {
            drmModeFreeResources(res);
            throw new Exception("drm: no connectors");
        }

        drmModeConnector* c;
        c = drmModeGetConnector(drmfd_, conId_);
        if (c == null)
        {
            drmModeFreeResources(res);
            //throw new Exception("drmModeGetConnector failed: " + std::string(ERRSTR));
            throw new Exception("drmModeGetConnector failed");
        }

        if (c->count_modes == 0)
        {
            drmModeFreeConnector(c);
            drmModeFreeResources(res);
            throw new Exception("connector supports no mode");
        }

        drmModeCrtc* crtc2 = drmModeGetCrtc(drmfd_, crtcId_);
        x_ = crtc2->x;
        y_ = crtc2->y;
        width_ = crtc2->width;
        height_ = crtc2->height;
        drmModeFreeCrtc(crtc2);

        drmModeFreeConnector(c);
        drmModeFreeResources(res);
    }
}