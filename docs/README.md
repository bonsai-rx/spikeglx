# Bonsai.SpikeGLX
[Bonsai](http://bonsai-rx.org/) package for interfacing with [SpikeGLX](https://billkarsh.github.io/SpikeGLX/).

## Description
Bonsai.SpikeGLX is designed to allow integration of SpikeGLX recordings into Bonsai workflows. This package provides the ability to 
- stream neural and non-neural data from SpikeGLX into Bonsai in real-time,
- set digital outputs on an NI device in use by SpikeGLX, and
- start and stop SpikeGLX recordings. 

## Getting Started
### Starting the SpikeGLX Remote Command Server
Bonsai.SpikeGLX requires SpikeGLX's remote command server to be running. See the [SpikeGLX-CPP-SDK Getting Started](https://github.com/billkarsh/SpikeGLX-CPP-SDK/blob/main/GettingStarted.txt) page for how to do this. Each Bonsai.SpikeGLX marble requires the IP address and port of the SpikeGLX command server to work. 

### Streaming SpikeGLX Data into Bonsai
Bonsai.SpikeGLX provides two ways to access data from SpikeGLX once an acquisition is running: 
- **SpikeFetch** will fetch a fixed-size buffer of the latest data from SpikeGLX whenever it receives an input signal (or at a fixed interval if no input is provided).
- **SpikeStream** will continuously stream data in fixed-size buffers from SpikeGLX.

The key difference between the two is that unlike SpikeFetch, SpikeStream will never skip or repeat elements in the buffers it emits. This is demonstrated below, where both marbles have been used to stream a 1Hz sinewave in 1s buffers. The output of SpikeFetch 'jumps' around as it misses or repeats samples, whereas the output of SpikeStream does not.

![SpikeFetch-vs-SpikeStream-Timed](https://github.com/FeeLab/Bonsai.SpikeGLX/assets/120409412/24fda4b8-fa1c-4089-8dee-a33c9126eed7)

### Setting Digital Outputs
The **DigitalOutput** marble may be used to control digital outputs on an NI DAQ system in use by SpikeGLX.

### Controlling SpikeGLX Recordings
SpikeGLX recordings may be started and stopped using the **TriggerRecording** marble, which can increment/decrement the gate and trigger values in SpikeGLX. It takes a tuple of two integers, which modify the gate and trigger, respectively, in the following way:
- -1: No change
- 0: Set low
- 1: Increment and set high

As of SpikeGLX Release v20230815-phase30, adding a fixed period of saved data before and after triggering for context is not implemented for remote triggering. An alternative is to use TTL controlled start and stop, with a digital output (controlled using the **DigitalOutput** marble) as the TTL signal. 

## Authors
Bonsai.SpikeGLX is developed by Jacob White of the [Fee Lab](https://feelaboratory.org/) at MIT.

## Version History
-0.1
  - Initial Release

## License
Use is subject to [MIT](https://opensource.org/license/mit/) license terms. 

SpikeGLX-CPP-SDK components are subject to [Janelia Research Campus Software Copyright 1.2 license](http://license.janelia.org/license) terms.

## Acknowledgments
The SpikeStream marble is inspired by the AudioCapture marble in [Bonsai.Audio](https://github.com/bonsai-rx/bonsai/tree/main/Bonsai.Audio).

