---
uid: spikeglx-fetch
title: Fetch
---

[`Fetch`] samples data from an ongoing SpikeGLX run. Data are sampled from one or more channels of a single SpikeGLX data stream. Data are emitted in a sample buffer, where each row corresponds to a channel, and each column to a sample from each of the channels. The order of the channels follows the order in which you specify the channels in the `Channels` property.

If no input source is specified, data is emitted asynchronously every time a new buffer is filled.
:::workflow
![Unsubscribed Fetch](~/workflows/GettingStarted-UnsubscribedFetch.bonsai)
:::

Alternatively, if an input observable sequence is provided, a new data buffer is emitted every time a new notification is emitted by the input source. 
:::workflow
![Subscribed Fetch](~/workflows/GettingStarted-SubscribedFetch.bonsai)
:::

<!--Reference Style Links -->
[`Fetch`]: xref:Bonsai.SpikeGLX.Fetch