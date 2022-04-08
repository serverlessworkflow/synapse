<p align="center">
  <img src="logos/synapse-color.png" height="350px" alt="Synapse Logo"/>
</p>

# Synapse

## About

Synapse is a vendor-neutral, free, open-source, and community-driven Workflow Management System (WFMS) implementing the [Serverless Workflow specification](https://github.com/serverlessworkflow/specification).

## Requirements

- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or later

*Optionally, and depending on which flavor of the Synapse Server you chose to run, you might require:*

- [Docker](https://www.docker.com/)
- [Kubernetes](https://kubernetes.io/)

## Server

### Installing

#### Native

The Synapse Server can run natively on Windows, Mac and Linux, without any dependencies aside from .NET. Event though it is the easier way to get started, it should only be used for tests purposes. For production, you should prefer the Docker or Kubernetes based setups.

To get started, just download the appropriate [release](https://github.com/serverlessworkflow/synapse/releases/latest) for your system, then start it using the following command:

```shell
dotnet run ./Synapse.Server.dll
```

For more information on how to configure a Native Synapse Server, please read the [docs]().

#### Docker

Docker is the recommended way to run the Synapse Server for those who do not want to host it on a Kubernetes cluster.

To run the server on Docker, simply execute the following command in your system's shell:

```shell
docker run ghcr.io/neuroglia-io/synapse:latest
```

For more information on how to configure Synapse for Docker, please read the [docs]().

#### Kubernetes

Coming soon

## User Interfaces

Synapse provides 2 different UIs for interacting with the server:

### GUI

The `Dashboard` is a Blazor Web Assembly (WASM) Graphical User Interface (GUI) that comes bundled with the Synapse Server. 

To get started, simply open a web browser and navigate to the Synapse Server's base url.

For more information on how to use the `Dashboard`, please read the [docs]().

### CLI

`synctl` is a Command Line Interface (CLI) used to interact with the Synapse Server. 

To get started, just download the appropriate [release](https://github.com/serverlessworkflow/synapse/releases/latest) for your system, then type the following command:

```shell
synctl --help
```

For more information on how to use `synctl`, please read the [docs]().

## Application Programing Interfaces

The Synapse Server is shipped with 3 different APIs, each addressing a different use-case. All the implementations of those APIs are supplied with their respective client library.

### Management API

The Synapse Management API is used to manage workflows and their instances.

Implementations:

- [x] HTTP REST
- [x] [GRPC](https://github.com/grpc/grpc-dotnet)
- [ ] WebSockets ([SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR))

### Monitoring API

The Synapse Monitoring API is used for real-time observability of workflows and their instances. It is used by the Dashboard to enable real-time updates.

Implementations:

- [ ] [GRPC](https://github.com/grpc/grpc-dotnet)
- [x] WebSockets ([SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR))

### Runtime API

The Synapse Runtime API is used by workers to run workflows and maintain their state. It preferably should not be used by anything else than runtime executors.

Implementations:

- [x] [GRPC](https://github.com/grpc/grpc-dotnet)
- [ ] WebSockets ([SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR))

## Community

We have a growing community working together to build a community-driven and vendor-neutral
workflow ecosystem. Community contributions are welcome and much needed to foster project growth.

See [here](community/contributors.md) for the list of community members that have contributed to the specification.

To learn how to contribute to the specification reference the ['how to contribute'](CONTRIBUTING.md) doc.

If you have any copyright questions when contributing to a CNCF project like this one,
reference the [Ownership of Copyrights in CNCF Project Contributions](https://github.com/cncf/foundation/blob/master/copyright-notices.md) doc.

### Code of Conduct

As contributors and maintainers of this project, and in the interest of fostering
an open and welcoming community, we pledge to respect all people who contribute
through reporting issues, posting feature requests, updating documentation,
submitting pull requests or patches, and other activities.

We are committed to making participation in this project a harassment-free experience for
everyone, regardless of level of experience, gender, gender identity and expression,
sexual orientation, disability, personal appearance, body size, race, ethnicity, age,
religion, or nationality.

See our full project Code of Conduct information [here](CODE-OF-CONDUCT.md).

## Repository Structure

Here is the outline of the repository to help navigate the specification
documents:

| File/Directory | Description | 
| --- | --- | 
| [LICENSE](LICENSE) | Specification License doc | 
| [OWNERS](OWNERS.md) | Defines the current maintainers and approvers | 
| [MAINTAINERS](MAINTAINERS.md) | Project Maintainers Info | 
| [GOVERNANCE](GOVERNANCE.md) | Project Governance Info | 
| [CONTRIBUTING](CONTRIBUTING.md) | Documentation on how to contribute to the project | 
| [CODE-OF-CONDUCT](code-of-conduct.md) | Defines the project's Code of Conduct | 
| [roadmap](roadmap/README.md) | Project Roadmap |
