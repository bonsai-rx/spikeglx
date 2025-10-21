# Getting Started
<<<<<<< HEAD
**Bonsai.SpikeGLX** is a [Bonsai](https://bonsai-rx.org/) package for interfacing with the [SpikeGLX](https://billkarsh.github.io/SpikeGLX/) recording software for [Neuropixels](https://www.neuropixels.org/). It is designed to supplement the core functionalities of SpikeGLX (*i.e.*, visualizing and saving data) and faciliate more complex experimental paradigms (*e.g.*, closed-loop neurofeedback, brain-computer interfaces, etc.). 
**Bonsai.SpikeGLX** provides three ways of interacting with an ongoing SpikeGLX run:
- streaming data from SpikeGLX into Bonsai with [`Fetch`];
=======
`Bonsai.SpikeGLX` is a [Bonsai](https://bonsai-rx.org/) package for interfacing with the [SpikeGLX](https://billkarsh.github.io/SpikeGLX/) recording software for [Neuropixels](https://www.neuropixels.org/). It is designed to supplement the core functionalities of SpikeGLX (*i.e.*, visualizing and saving data) and faciliate more complex experimental paradigms (*e.g.*, closed-loop neurofeedback, brain-computer interfaces, etc.). 
`Bonsai.SpikeGLX` provides three ways of interacting with an ongoing SpikeGLX run:
- streaming data from SpikeGLX into Bonsai with [`SpikeGLXInput`];
>>>>>>> 362c9d7 (Use backticks (`) to highlight all package names.)
- controlling SpikeGLX digital output lines with [`DigitalOutput`]; and
- starting and stopping recordings with [`Trigger`].

A brief summary of the functionality of each of these operators is provided here. A more detailed description of each may be found on the associated Reference page.
> [!NOTE]
> All `Bonsai.SpikeGLX` operators require SpikeGLX's remote command server to be running. A full description of how to do this can be found in the [SpikeGLX User Manual](https://billkarsh.github.io/SpikeGLX/Sgl_help/UserManual.html#remote-command-servers). Make sure to note the `Host` (IP Address) and `Port` the command server is listening on. These are properties that must be set for each `Bonsai.SpikeGLX` operator.

## Stream Data from SpikeGLX into Bonsai
[!include[Fetch](~/articles/spikeglx-fetch.md)]

## Control Digital Outputs
[!include[DigitalOutput](~/articles/spikeglx-digitaloutput.md)]

## Save Data
SpikeGLX is expressly designed for saving Neuropixels data. As a result, it is recommended to use SpikeGLX's built-in capabilities to save your data; however, `Bonsai.SpikeGLX` provides ways to control which parts of a run SpikeGLX should save. If a SpikeGLX run is configured to use "remote controlled start and stop" triggering, [`Trigger`] may be used to set gate and trigger values. 

[!include[Trigger](~/articles/spikeglx-trigger)]

> [!NOTE]
> SpikeGLX also provides the ability to trigger recordings using a TTL line. In some cases, this may be preferred over using [`Trigger`].

If you do wish to save data you have streamed into Bonsai using [`Fetch`], you can use the [`MatrixWriter`] operator from the `Bonsai.Dsp` package. 

<!--Reference Style Links -->
[`Fetch`]: xref:Bonsai.SpikeGLX.Fetch
[`DigitalOutput`]: xref:Bonsai.SpikeGLX.DigitalOutput
[`Trigger`]: xref:Bonsai.SpikeGLX.Trigger
[`Timer`]: xref:Bonsai.Reactive.Timer
[`MatrixWriter`]: xref:Bonsai.Dsp.MatrixWriter