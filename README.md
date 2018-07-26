# FFSharp
Manager .NET wrapper for FFmpeg based on FFmpeg.AutoGen.

## Motivation

Having had to deal with a lot of unmanaged / managed interop in C#, specifically using the **FFmpeg** library for transcoding, I discovered a lack of tools for the job. The [**FFmpeg.AutoGen**](https://github.com/Ruslan-B/FFmpeg.AutoGen) project does an outstanding job at providing the necessary definitions; but nothing more. This leaves you with the best starting point for creating performant (funny, I know) code, but also lots of trouble. In addition to that the FFmpeg docs are famously vague on certain things, and the exception behaviour you would need to model / the error modes are rather unclear sometimes.

## Goals

**FFSharp** aims to provide both in a reasonable manner. Instead of being ultimatively generic it wraps more commonly used types and provides facades for them. It does this in three ways: as internal (*Native*) helpers that further document and wrap library calls with assertions and such, and public (*Managed*) types for the more commonly used types, providing a neat .NET worthy framework interface that is completely safe. Lastly are the public *Prefab* types that provide standard solutions implemented with internal access like de-/en-code/filter pipelines.

Depending on the usage scenario you can now either fork the library and build upon the internals, or stick with the provided managed solutions depending on speed requirements and implemented functionality.

## Status

Current status:

> Note that this list is incomplete, just immediately planned things of the top of all I need.

Most important goals are the decoding pipeline for MPEG2 in software and with DXVA2 on Windows, with WGL_NV_DX_interop ready support for state introspection (A reference implementation using hand-crafted code exists.).

### Native

- [ ] AVUtil lib
  - [x] AVDictionary
  - [x] AVFrame
  - [x] AVBufferRef
  - [ ] AVOption (on hold)
  - [ ] AVTreeNode (**FFmpeg.AutoGen**: no bindings)
  
- [ ] AVFormat lib
  - [x] AVIOContext
  - [ ] AVFormatContext
  - [ ] AVInputFormat
  - [ ] AVOutputFormat
  - [ ] AVStream
 
- [ ] AVCodec lib
  - [ ] AVCodec
  - [ ] AVCodecContext
  - [ ] AVPacket
  - [ ] AVHWDeviceContext

### Managed

- [x] Dictionary
- [ ] Codec
- [ ] Container
- [ ] Muxer
- [ ] Demuxer
- [ ] Stream
- [ ] Transcode
- [ ] HardwareDevice

### Prefab

- [ ] Pipeline (reference implementation from previous project exists)
  - [ ] Channel
  - [ ] PacketReader
  - [ ] PacketWriter
  - [ ] Encode
  - [ ] Decode
  - [ ] Filter
  - [ ] Transfer
