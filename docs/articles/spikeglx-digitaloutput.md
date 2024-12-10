---
uid: spikeglx-digitaloutput
title: DigitalOutput
---

[`DigitalOutput`] writes a logical value to one or more digital output lines on a NI-DAQ device in use by SpikeGLX. Digital lines are specified in the `Channels` property as a comma separated list of digital lines, *e.g.*, `Dev6/port0/line2,Dev6/port0/line5`.

> [!WARNING]
> Unlike `Bonsai.DAQmx`'s [`DigitalOutput`](xref:Bonsai.DAQmx.DigitalOutput), `Bonsai.SpikeGLX`'s [`DigitalOutput`] writes **the same** logical value to all of its channels.

<!--Reference Style Links -->
[`DigitalOutput`]: xref:Bonsai.SpikeGLX.DigitalOutput