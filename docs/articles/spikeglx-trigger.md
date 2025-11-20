---
uid: spikeglx-trigger
title: Trigger
---

[`Trigger`] takes as input a `Tuple` of two values, which modify the gate and trigger, respectively, in the following way:
- -1: No change
- 0: Set low
- 1: Increment and set high

For example, the below workflow starts a new gate immediately, and then toggles the trigger every five seconds. This results in SpikeGLX saving five-second periods of the run every ten seconds. 
:::workflow
![Trigger](../workflows/GettingStarted-Trigger.bonsai)
:::

<!--Reference Style Links -->
[`Trigger`]: xref:Bonsai.SpikeGLX.Trigger