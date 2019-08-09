// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    /// <summary>
    /// MimeType map is class help get file extension from its mime type and get mime type through it name.
    /// </summary>
    public static class MimeTypesMap
    {
        /// <summary>
        /// Lazy instance of an Dictionary which store the extension/mimetype pairs.
        /// All mime type definition: http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types.
        /// Remove the type WeChat won't support.
        /// </summary>
        private static readonly Lazy<Dictionary<string, string>> MimeTypeMap = new Lazy<Dictionary<string, string>>(() => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["adp"] = "audio/adpcm",
            ["au"] = "audio/basic",
            ["snd"] = "audio/basic",
            ["mid"] = "audio/midi",
            ["midi"] = "audio/midi",
            ["kar"] = "audio/midi",
            ["rmi"] = "audio/midi",
            ["m4a"] = "audio/mp4",
            ["mp4a"] = "audio/mp4",
            ["mp3"] = "audio/mpeg",
            ["mpga"] = "audio/mpeg",
            ["mp2"] = "audio/mpeg",
            ["mp2a"] = "audio/mpeg",
            ["m2a"] = "audio/mpeg",
            ["m3a"] = "audio/mpeg",
            ["oga"] = "audio/ogg",
            ["ogg"] = "audio/ogg",
            ["spx"] = "audio/ogg",
            ["s3m"] = "audio/s3m",
            ["sil"] = "audio/silk",
            ["uva"] = "audio/vnd.dece.audio",
            ["uvva"] = "audio/vnd.dece.audio",
            ["eol"] = "audio/vnd.digital-winds",
            ["dra"] = "audio/vnd.dra",
            ["dts"] = "audio/vnd.dts",
            ["dtshd"] = "audio/vnd.dts.hd",
            ["lvp"] = "audio/vnd.lucent.voice",
            ["pya"] = "audio/vnd.ms-playready.media.pya",
            ["ecelp4800"] = "audio/vnd.nuera.ecelp4800",
            ["ecelp7470"] = "audio/vnd.nuera.ecelp7470",
            ["ecelp9600"] = "audio/vnd.nuera.ecelp9600",
            ["rip"] = "audio/vnd.rip",
            ["weba"] = "audio/webm",
            ["aac"] = "audio/x-aac",
            ["aif"] = "audio/x-aiff",
            ["aiff"] = "audio/x-aiff",
            ["aifc"] = "audio/x-aiff",
            ["caf"] = "audio/x-caf",
            ["flac"] = "audio/x-flac",
            ["mka"] = "audio/x-matroska",
            ["m3u"] = "audio/x-mpegurl",
            ["wax"] = "audio/x-ms-wax",
            ["wma"] = "audio/x-ms-wma",
            ["ram"] = "audio/x-pn-realaudio",
            ["ra"] = "audio/x-pn-realaudio",
            ["rmp"] = "audio/x-pn-realaudio-plugin",
            ["wav"] = "audio/x-wav",
            ["amr"] = "audio/amr",
            ["xm"] = "audio/xm",
            ["bmp"] = "image/bmp",
            ["cgm"] = "image/cgm",
            ["g3"] = "image/g3fax",
            ["gif"] = "image/gif",
            ["ief"] = "image/ief",
            ["jpg"] = "image/jpeg",
            ["jpeg"] = "image/jpeg",
            ["jpe"] = "image/jpeg",
            ["ktx"] = "image/ktx",
            ["png"] = "image/png",
            ["btif"] = "image/prs.btif",
            ["sgi"] = "image/sgi",
            ["svg"] = "image/svg+xml",
            ["svgz"] = "image/svg+xml",
            ["tiff"] = "image/tiff",
            ["tif"] = "image/tiff",
            ["psd"] = "image/vnd.adobe.photoshop",
            ["uvi"] = "image/vnd.dece.graphic",
            ["uvvi"] = "image/vnd.dece.graphic",
            ["uvg"] = "image/vnd.dece.graphic",
            ["uvvg"] = "image/vnd.dece.graphic",
            ["djvu"] = "image/vnd.djvu",
            ["djv"] = "image/vnd.djvu",
            ["sub"] = "image/vnd.dvb.subtitle",
            ["dwg"] = "image/vnd.dwg",
            ["dxf"] = "image/vnd.dxf",
            ["fbs"] = "image/vnd.fastbidsheet",
            ["fpx"] = "image/vnd.fpx",
            ["fst"] = "image/vnd.fst",
            ["mmr"] = "image/vnd.fujixerox.edmics-mmr",
            ["rlc"] = "image/vnd.fujixerox.edmics-rlc",
            ["mdi"] = "image/vnd.ms-modi",
            ["wdp"] = "image/vnd.ms-photo",
            ["npx"] = "image/vnd.net-fpx",
            ["wbmp"] = "image/vnd.wap.wbmp",
            ["xif"] = "image/vnd.xiff",
            ["webp"] = "image/webp",
            ["3ds"] = "image/x-3ds",
            ["ras"] = "image/x-cmu-raster",
            ["cmx"] = "image/x-cmx",
            ["fh"] = "image/x-freehand",
            ["fhc"] = "image/x-freehand",
            ["fh4"] = "image/x-freehand",
            ["fh5"] = "image/x-freehand",
            ["fh7"] = "image/x-freehand",
            ["ico"] = "image/x-icon",
            ["sid"] = "image/x-mrsid-image",
            ["pcx"] = "image/x-pcx",
            ["pic"] = "image/x-pict",
            ["pct"] = "image/x-pict",
            ["pnm"] = "image/x-portable-anymap",
            ["pbm"] = "image/x-portable-bitmap",
            ["pgm"] = "image/x-portable-graymap",
            ["ppm"] = "image/x-portable-pixmap",
            ["rgb"] = "image/x-rgb",
            ["tga"] = "image/x-tga",
            ["xbm"] = "image/x-xbitmap",
            ["xpm"] = "image/x-xpixmap",
            ["xwd"] = "image/x-xwindowdump",
            ["3gp"] = "video/3gpp",
            ["3g2"] = "video/3gpp2",
            ["h261"] = "video/h261",
            ["h263"] = "video/h263",
            ["h264"] = "video/h264",
            ["jpgv"] = "video/jpeg",
            ["jpm"] = "video/jpm",
            ["jpgm"] = "video/jpm",
            ["mj2"] = "video/mj2",
            ["mjp2"] = "video/mj2",
            ["mp4"] = "video/mp4",
            ["mp4v"] = "video/mp4",
            ["mpg4"] = "video/mp4",
            ["mpeg"] = "video/mpeg",
            ["mpg"] = "video/mpeg",
            ["mpe"] = "video/mpeg",
            ["m1v"] = "video/mpeg",
            ["m2v"] = "video/mpeg",
            ["ogv"] = "video/ogg",
            ["qt"] = "video/quicktime",
            ["mov"] = "video/quicktime",
            ["uvh"] = "video/vnd.dece.hd",
            ["uvvh"] = "video/vnd.dece.hd",
            ["uvm"] = "video/vnd.dece.mobile",
            ["uvvm"] = "video/vnd.dece.mobile",
            ["uvp"] = "video/vnd.dece.pd",
            ["uvvp"] = "video/vnd.dece.pd",
            ["uvs"] = "video/vnd.dece.sd",
            ["uvvs"] = "video/vnd.dece.sd",
            ["uvv"] = "video/vnd.dece.video",
            ["uvvv"] = "video/vnd.dece.video",
            ["dvb"] = "video/vnd.dvb.file",
            ["fvt"] = "video/vnd.fvt",
            ["mxu"] = "video/vnd.mpegurl",
            ["m4u"] = "video/vnd.mpegurl",
            ["pyv"] = "video/vnd.ms-playready.media.pyv",
            ["uvu"] = "video/vnd.uvvu.mp4",
            ["uvvu"] = "video/vnd.uvvu.mp4",
            ["viv"] = "video/vnd.vivo",
            ["webm"] = "video/webm",
            ["f4v"] = "video/x-f4v",
            ["fli"] = "video/x-fli",
            ["flv"] = "video/x-flv",
            ["m4v"] = "video/x-m4v",
            ["mkv"] = "video/x-matroska",
            ["mk3d"] = "video/x-matroska",
            ["mks"] = "video/x-matroska",
            ["mng"] = "video/x-mng",
            ["asf"] = "video/x-ms-asf",
            ["asx"] = "video/x-ms-asf",
            ["vob"] = "video/x-ms-vob",
            ["wm"] = "video/x-ms-wm",
            ["wmv"] = "video/x-ms-wmv",
            ["wmx"] = "video/x-ms-wmx",
            ["wvx"] = "video/x-ms-wvx",
            ["avi"] = "video/x-msvideo",
            ["movie"] = "video/x-sgi-movie",
            ["smv"] = "video/x-smv",
        });

        /// <summary>
        /// Get file extension by its mime type.
        /// </summary>
        /// <param name="mimeType">File mime type string.</param>
        /// <returns>File extension.</returns>
        public static string GetExtension(string mimeType)
        {
            var ext = MimeTypeMap.Value.FirstOrDefault(x => x.Value == mimeType).Key;
            return ext;
        }

        /// <summary>
        /// Get mime type through the file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>file mimetype.</returns>
        public static string GetMimeType(string fileName)
        {
            var ext = fileName;
            var index = ext.LastIndexOf('.');
            if (index != -1 && ext.Length > index + 1)
            {
#pragma warning disable CA1308 // file extation should be lower case.
                ext = fileName.Substring(index + 1).ToLowerInvariant();
#pragma warning restore CA1308 // file extation should be lower case.
            }

            if (MimeTypeMap.Value.TryGetValue(ext, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
