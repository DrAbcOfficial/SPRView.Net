/*
 * sprview_core.h - C ABI of the SPRView.Net core renderer.
 *
 * The shared library is produced with NativeAOT:
 *
 *     dotnet publish src/SPRView.Net.Core -c Release -r <rid> \
 *         -p:NativeLib=Shared -p:PublishAot=true
 *
 * Outputs: sprview_core.dll (Windows), sprview_core.so (Linux),
 *          sprview_core.dylib (macOS).
 *
 * All functions are thread safe as long as each handle is used by a single
 * thread at a time. Every function returning int32_t reports a spr_result
 * code; 0 means success. Handles and buffers returned to native callers must
 * be released with sprview_close / sprview_free_buffer.
 */
#ifndef SPRVIEW_CORE_H
#define SPRVIEW_CORE_H

#include <stdint.h>

#if defined(_WIN32)
#  define SPRVIEW_EXPORT __declspec(dllexport)
#else
#  define SPRVIEW_EXPORT __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef void* spr_handle; /* opaque document handle, 0 == invalid */

typedef struct spr_info
{
    uint32_t frames;          /* number of stored frames          */
    uint32_t max_frame_width; /* frame grid width                 */
    uint32_t max_frame_height;/* frame grid height                */
    int32_t  type;            /* spr_type                         */
    int32_t  format;          /* spr_format                       */
    int32_t  synchronization; /* spr_synchron                     */
    float    bound_radius;
    float    beam_length;
    int32_t  palette_size;    /* colors stored in the palette     */
} spr_info;

typedef enum spr_type
{
    SPR_TYPE_PARALLEL_UPRIGHT   = 0,
    SPR_TYPE_FACING_UPRIGHT     = 1,
    SPR_TYPE_PARALLEL           = 2,
    SPR_TYPE_ORIENTED           = 3,
    SPR_TYPE_PARALLEL_ORIENTED  = 4
} spr_type;

typedef enum spr_format
{
    SPR_FORMAT_NORMAL     = 0,
    SPR_FORMAT_ADDITIVE   = 1,
    SPR_FORMAT_INDEX_ALPHA= 2,
    SPR_FORMAT_ALPHA_TEST = 3
} spr_format;

typedef enum spr_synchron
{
    SPR_SYNCHRONIZATION_SYNC   = 0,
    SPR_SYNCHRONIZATION_RANDOM = 1
} spr_synchron;

typedef enum spr_result
{
    SPR_RESULT_OK              =  0,
    SPR_RESULT_INVALID_HANDLE  = -1,
    SPR_RESULT_FRAME_OUT_OF_RANGE = -2,
    SPR_RESULT_BUFFER_TOO_SMALL   = -3,
    SPR_RESULT_IO_ERROR        = -4,
    SPR_RESULT_INVALID_ARGUMENT   = -5,
    SPR_RESULT_UNKNOWN_ERROR   = -6
} spr_result;

/* Returns the ABI revision implemented by this library. */
SPRVIEW_EXPORT int32_t sprview_abi_version(void);

/* Opens a .spr file from a NUL terminated UTF-8 path. 0 on failure. */
SPRVIEW_EXPORT spr_handle sprview_open(const char* path_utf8);

/* Opens a .spr document from memory. 0 on failure. */
SPRVIEW_EXPORT spr_handle sprview_open_memory(const uint8_t* data, intptr_t length);

/* Releases a handle returned by sprview_open*. NULL/0 is allowed. */
SPRVIEW_EXPORT void sprview_close(spr_handle handle);

/* Fills *info with document metadata. */
SPRVIEW_EXPORT int32_t sprview_get_info(spr_handle handle, spr_info* info);

/* Frame geometry; any out pointer may be NULL. */
SPRVIEW_EXPORT int32_t sprview_get_frame_info(spr_handle handle, int32_t frame,
    int32_t* width, int32_t* height, int32_t* origin_x, int32_t* origin_y);

/* Copies frame `frame` as tightly packed RGBA8888 pixels into `buffer`.
 * `buffer_size` must be at least width * height * 4. */
SPRVIEW_EXPORT int32_t sprview_read_frame_rgba(spr_handle handle, int32_t frame,
    uint8_t* buffer, intptr_t buffer_size);

/* Renders frame `frame` scaled (nearest neighbor) to fit `max_size` and
 * encodes it as PNG. On success *out_data receives a malloc'd buffer and
 * *out_size its length; free it with sprview_free_buffer. */
SPRVIEW_EXPORT int32_t sprview_render_png(spr_handle handle, int32_t frame,
    int32_t max_size, uint8_t** out_data, intptr_t* out_size);

/* Releases a buffer returned by sprview_render_png. NULL is allowed. */
SPRVIEW_EXPORT void sprview_free_buffer(void* buffer);

#ifdef __cplusplus
}
#endif

#endif /* SPRVIEW_CORE_H */
