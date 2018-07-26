# FFSharp.Native

## LibAVUtil

### `AVBuffer`

Class `AVBuffer`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `av_buffer_alloc`         | `Alloc`           | :heavy_check_mark:    | Assert: `Size > 0`. Integrated alloc check. |
| `av_buffer_allocz`        | `AllocZ`          | :heavy_check_mark:    | Assert: `Size > 0`. Integrated alloc check. |
| `av_buffer_create`        | `Create`          | :heavy_check_mark:    | Assert: `Size > 0`. Wraps `Action<Fixed, Fixed<byte>>` free delegate. Uses `AVBufferFlags`. Integrated alloc check. |
| `av_buffer_default_free`  | -                 | ~~EXCLUDED~~          | Not necessary to expose to the user. |
| `av_buffer_get_ref_count` | `GetRefCount`     | :heavy_check_mark:    | - |
| `av_buffer_get_opaque`    | -                 | :x:                   | - |
| `av_buffer_is_writable`   | `IsWritable`      | :heavy_check_mark:    | - |
| `av_buffer_make_writable` | `MakeWritable`    | :heavy_check_mark:    | - |
| `av_buffer_realloc`       | `Realloc`         | :heavy_check_mark:    | Assert: `Size > 0`. |
| `av_buffer_ref`           | `Ref`             | :heavy_check_mark:    | Integrated error check. |
| `av_buffer_unref`         | `Unref`           | :heavy_check_mark:    | - |

### `AVBufferPool`

Class `AVBufferPool`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `av_buffer_pool_get`      | - | :x:   | - |
| `av_buffer_pool_init`     | - | :x:   | - |
| `av_buffer_pool_init2`    | - | :x:   | - |
| `av_buffer_pool_uninit`   | - | :x:   | - |

### `AVDictionary`

Class `AVDictionary` 

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| -                         | `Alloc`           | :heavy_check_mark:    | For uniformity. |
| `av_dict_copy`            | `Copy`            | :heavy_check_mark:    | Uses `AVDictionaryFlags`. |
| `av_dict_count`           | `Count`           | :heavy_check_mark:    | - |
| `av_dict_free`            | `Free`            | :heavy_check_mark:    | - |
| `av_dict_get`             | `Get`             | :heavy_check_mark:    | Prohibits `AV_DICT_DONT_STRDUP_KEY`. |
| -                         | `GetEnumerator`   | :heavy_check_mark:    | `IEnumerator` wrapper for `Get`. |
| `av_dict_get_string`      | `GetString`       | :heavy_check_mark:    | Assert: Separators are valid. Buffer handling. |
| `av_dict_parse_string`    | `ParseString`     | :heavy_check_mark:    | Assert: Separators are valid. Uses `AVDictionaryFlags`. |
| `av_dict_set`             | `Set`             | :heavy_check_mark:    | Prohibits `AV_DICT_DONT_STRDUP_KEY` and `AV_DICT_DONT_STRDUP_VAL`. Uses `AVDictionaryFlags`. |
| `av_dict_set_int`         | -                 | :x:                   | - |
| -                         | `ToPair`          | :heavy_check_mark:    | Helper for `AVDictionaryEntry`. |

### `AVFrame`

Class `AVFrame`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `av_frame_alloc`                  | `Alloc`               | :heavy_check_mark:    | Integrated alloc check. |
| `av_frame_apply_cropping`         | `ApplyCropping`       | :heavy_check_mark:    | Uses `AVFrameCropFlags`. |
| `av_frame_clone`                  | `Clone`               | :heavy_check_mark:    | Integrated alloc check. |
| `av_frame_copy`                   | `Copy`                | :heavy_check_mark:    | - |
| `av_frame_copy_props`             | `CopyProps`           | :heavy_check_mark:    | - |
| `av_frame_free`                   | `Free`                | :heavy_check_mark:    | - |
| `av_frame_get_buffer`             | `GetBuffer`           | :heavy_check_mark:    | - |
| `av_frame_get_plane_buffer`       | `GetPlaneBuffer`      | :heavy_check_mark:    | Integrated alloc check. |
| `av_frame_get_side_data`          | `GetSideData`         | :heavy_check_mark:    | - |
| `av_frame_is_writable`            | `IsWritable`          | :heavy_check_mark:    | - |
| `av_frame_make_writable`          | `MakeWritable`        | :heavy_check_mark:    | - |
| `av_frame_move_ref`               | `MoveRef`             | :heavy_check_mark:    | - |
| `av_frame_new_side_data`          | `NewSideData`         | :heavy_check_mark:    | Integrated alloc check. |
| `av_frame_new_side_data_from_buf` | `NewSideDataFromBuf`  | :heavy_check_mark:    | Integrated alloc check. |
| `av_frame_ref`                    | `Ref`                 | :heavy_check_mark:    | - |
| `av_frame_remove_side_data`       | `RemoveSideData`      | :heavy_check_mark:    | - |
| `av_frame_side_data_name`         | `SideDataName`        | :heavy_check_mark:    | - |
| `av_frame_unref`                  | `Unref`               | :heavy_check_mark:    | - |

### `AVOption`

Class `AVOption`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `av_opt_child_class_next`             | - | :x:   | - |
| `av_opt_child_next`                   | - | :x:   | - |
| `av_opt_copy`                         | - | :x:   | - |
| `av_opt_find`                         | - | :x:   | - |
| `av_opt_find2`                        | - | :x:   | - |
| `av_opt_flag_is_set`                  | - | :x:   | - |
| `av_opt_free`                         | - | :x:   | - |
| `av_opt_freep_ranges`                 | - | :x:   | - |
| `av_opt_get_key_value`                | - | :x:   | - |
| `av_opt_is_set_to_default_by_name`    | - | :x:   | - |
| `av_opt_next`                         | - | :x:   | - |
| `av_set_options_string`               | - | :x:   | - |
| `av_opt_ptr`                          | - | :x:   | - |
| `av_opt_query_ranges`                 | - | :x:   | - |
| `av_opt_query_ranges_default`         | - | :x:   | - |
| `av_opt_set_from_string`              | - | :x:   | - |
| `av_opt_set_defaults`                 | - | :x:   | - |
| `av_opt_set_defaults2`                | - | :x:   | - |
| `av_opt_set_dict`                     | - | :x:   | - |
| `av_opt_set_dict2`                    | - | :x:   | - |
| `av_opt_serialize`                    | - | :x:   | - |
| `av_opt_show2`                        | - | :x:   | - |

### `AVTree`

Class `AVTree`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `av_tree_node_alloc`  | `Alloc`   | :x:   | - |
| `av_tree_find`        | `Find`    | :x:   | - |
| `av_tree_destroy`     | `Free`    | :x:   | - |
| `av_tree_insert`      | `Insert`  | :x:   | - |
| `av_tree_enumerate`   | `Visit`   | :x:   | - |

## LibAVFormat

### `AVIOContext`

Class `AVIOContext`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `avio_accept`         | -                             | :x:                   | - |
| `avio_alloc_context`  | `Alloc`                       | :heavy_check_mark:    | Assert: `BufferSize > 0`. Wraps `Func<Fixed, Fixed<byte>, int, int>` read and write delegates. Wraps `Func<Fixed, long, AVIOSeekOrigin, AVIOSeekFlags, long>` seek delegate. Integrated alloc check. |
| `avio_close`          | `Close`                       | :heavy_check_mark:    | - |
| `avio_closep`         | -                             | ~~EXCLUDED~~          | Replaced by `Close`. |
| `avio_feof`           | `Feof`                        | :heavy_check_mark:    | - |
| `avio_fush`           | `Flush`                       | :heavy_check_mark:    | - |
| `avio_get_dyn_buf`    | -                             | :x:                   | - |
| `avio_hanshake`       | -                             | :x:                   | - |
| `avio_context_free`   | `Free`                        | :heavy_check_mark:    | - |
| `avio_close_dyn_buf`  | -                             | :x:                   | - |
| `avio_enum_protocols` | `LibAVFormat.EnumProtocols`   | :heavy_check_mark:    | Wrapped as `IEnumerator`. |
| `avio_open`           | -                             | ~~EXCLUDED~~          | Replaced by `Open`. |
| `avio_open2`          | `Open`                        | :heavy_check_mark:    | Uses `AVIOOpenMode` and `AVIOFlags`. |
| `avio_open_dyn_buf`   | -                             | :x:                   | - |
| `avio_pause`          | -                             | :x:                   | - |
| `avio_printf`         | -                             | :x:                   | - |
| `avio_read`           | -                             | :x:                   | - |
| `avio_read_partial`   | -                             | :x:                   | - |
| `avio_read_to_bprint` | -                             | :x:                   | - |
| `avio_seek`           | `Seek`                        | :heavy_check_mark:    | Uses `AVIOSeekOrigin` and  `AVIOSeekFlags`. |
| `avio_seek_time`      | -                             | :x:                   | - |
| `avio_size`           | `Size`                        | :heavy_check_mark:    | - |
| `avio_skip`           | `Skip`                        | :heavy_check_mark:    | - |
| `avio_tell`           | `Tell`                        | :heavy_check_mark:    | - |
| `avio_write`          | -                             | :x:                   | - |
| `avio_write_marker`   | -                             | :x:                   | - |

### `AVInputFormat`

Class `AVInputFormat`

| Name (C) | Name (C#) | Status | Remarks |
| --- | --- | :---: | --- |
| `avformat_close_input`        | `Close`           | :heavy_check_mark:    | - |
| `av_find_input_format`        | `Find`            | :heavy_check_mark:    | - |
| `av_find_best_stream`         | `FindBestStream`  | :heavy_check_mark:    | Assert: `Wanted and Related streams are in range`. |
| `av_find_program_from_stream` | -                 | :x:                   | - |
| `avformat_find_stream_info`   | `FindStreamInfo`  | :heavy_check_mark:    | Replaced options with array. Assert: `Options match in length if specified`. |
| `avformat_flush`              | `Flush`           | :heavy_check_mark:    | - |
| `avformat_open_input`         | `Open`            | :heavy_check_mark:    | - |
| `av_probe_input_buffer`       | -                 | :x:                   | - |
| `av_probe_input_buffer2`      | -                 | :x:                   | - |
| `av_probe_input_format`       | -                 | ~~EXCLUDED~~          | Replaced by `Probe`. |
| `av_probe_input_format2`      | -                 | ~~EXCLUDED~~          | Replaced by `Probe`. |
| `av_probe_input_format3`      | `Probe`           | :heavy_check_mark:    | - |
| `av_prgram_add_stream_index`  | -                 | :x:                   | - |
| `av_read_frame`               | `ReadFrame`       | :heavy_check_mark:    | EOF handling. |
| `av_read_pause`               | -                 | :x:                   | - |
| `av_read_play`                | -                 | :x:                   | - |
| `av_seek_frame`               | -                 | ~~EXCLUDED~~          | Replaced by `Seek`. |
| `avformat_seek_file`          | `Seek`            | :heavy_check_mark:    | Uses `AVSeekFlags`. Assert: `Flag combination is valid`. Assert: `Stream in range`. |

## LibAVCodec
