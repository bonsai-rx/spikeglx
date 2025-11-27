---
uid: spikeglx-digitaloutput
title: DigitalOutput
---

[`DigitalOutput`] writes a logical value to one or more digital output lines on a NI-DAQ device through SpikeGLX. Digital lines are specified in the `Channels` property as a comma separated list, (*e.g.*, `Dev6/port0/line0:1,Dev6/port0/line5`).

If an input is provided as a [`Boolean`], then all lines are set to the logical value of the input. For example, the below workflow toggles two digital lines between high and low every second. 
:::workflow
![DigitalOutputBoolean](../workflows/GettingStarted-DigitalOutput-Boolean.bonsai)
:::

Alternatively, an input may be provided as a [`UInt32`] bitmask. In this case, each line listed in `Channels` will be set to the value of its associated bit in the bitmask. For example, the below workflow cycles through all combinations of states for two digital lines.
:::workflow
![DigitalOutputBitmask](../workflows/GettingStarted-DigitalOutput-Bitmask.bonsai)
:::

<!--Reference Style Links -->
[`DigitalOutput`]: xref:Bonsai.SpikeGLX.DigitalOutput
[`Boolean`]: xref:System.Boolean
[`UInt32`]: xref:System.UInt32