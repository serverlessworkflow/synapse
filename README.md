<p align="center">
  <img src="assets/images/transparent_logo.png" height="350px" alt="Synapse Logo"/>
</p>

---

[![Publish](https://github.com/serverlessworkflow/synapse/actions/workflows/publish.yml/badge.svg)](https://github.com/serverlessworkflow/synapse/actions/workflows/publish.yml)
[![Release](https://img.shields.io/github/v/release/serverlessworkflow/synapse?include_prereleases)](https://github.com/serverlessworkflow/synapse/releases/latest)
[![Release](https://img.shields.io/github/release-date/serverlessworkflow/synapse?color=blueviolet)](https://github.com/serverlessworkflow/synapse/releases/latest)
[![Commits since latest](https://img.shields.io/github/commits-since/serverlessworkflow/synapse/latest)](https://github.com/serverlessworkflow/synapse/commits/)
[![Commits since latest](https://img.shields.io/github/last-commit/serverlessworkflow/synapse?color=blueviolet)](https://github.com/serverlessworkflow/synapse/commits/)
[![License](https://img.shields.io/github/license/serverlessworkflow/synapse?label=License&color=important)](https://github.com/serverlessworkflow/synapse/blob/main/LICENSE)

## About

Synapse is a vendor-neutral, open-source, and community-driven Workflow Management System (WFMS) designed to implement the [Serverless Workflow specification](https://github.com/serverlessworkflow/specification). 

It enables developers and organizations to define and execute workflows effortlessly using a high-level, intuitive Domain Specific Language (DSL). 

With Synapse, you can create powerful workflows that are cloud vendor-agnostic, easily scalable, and highly customizable.

### Features

- **Easy to Use**: The Serverless Workflow DSL is designed for universal understanding, enabling users to quickly grasp workflow concepts and create complex workflows effortlessly.
- **Event Driven**: Seamlessly integrate events into workflows with support for various formats, including CloudEvents, allowing for event-driven workflow architectures.
- **Service Oriented**: Integrate seamlessly with service-oriented architectures, allowing workflows to interact with various services over standard application protocols like HTTP, gRPC, OpenAPI, AsyncAPI, and more.
- **FaaS Centric**: Invoke functions hosted on various platforms within workflows, promoting a function-as-a-service (FaaS) paradigm and enabling microservices architectures.
- **Timely**: Define timeouts for workflows and tasks to manage execution duration effectively.
- **Fault Tolerant**: Easily define error handling strategies to manage and recover from errors that may occur during workflow execution, ensuring robustness and reliability.
- **Schedulable**: Schedule workflows using CRON expressions or trigger them based on events, providing control over workflow execution timing.
- **Interoperable**: Integrates seamlessly with different services and resources.
- **Robust**: Offers features such as conditional branching, event handling, and looping constructs.
- **Scalable**: Promotes code reusability, maintainability, and scalability across different environments.
- **Cross-Platform**: Runs on various operating systems, providing flexibility and ease of integration.

### Microservices

Synapse is composed of several specialized applications, allowing for atomic scalability, resilience, and ease of maintenance:

- **API Server**: Serves an HTTP API to manage Synapse resources, and optionally serves the **Dashboard**, which is Synapse's Graphical User Interface.
- **Operator**: Controls workflows and workflow instances, and starts workflow runners.
- **Runner**: Executes a single instance of a workflow.
- **Correlator**: Performs Complex Event Processing (CEP) and correlates ingested events.
- **CLI**: Allows interaction with the Synapse API via the command line interface.

## Getting Started

### Prerequisites

[Docker]() or [Kubernetes](), depending on the container platform you wish to use.

### Installation

The simplest way to get started is by using the provided Docker Compose setup. 

1. Clone the Synapse repository:

    ```bash
    git clone https://github.com/serverlessworkflow/synapse.git
    ```

2. Navigate to the Docker Compose directory:

    ```bash
    cd synapse/deployments/docker-compose
    ```

3. Build the Docker images:

    ```bash
    docker-compose build
    ```

4. Start the services using Docker Compose:

    ```bash
    docker-compose up
    ```

This will pull the necessary Docker images and start the Synapse services as defined in the `docker-compose.yml` file. You can then access the Synapse API and dashboard as configured.

### Run using `synctl` Command-line Interface

First, set up the Synapse API server to use with `synctl`:

```bash
synctl config set-api default -server=http://localhost:8080
```

Then, create a new file with the definition of the workflow to create:

```yaml
# greeter.yaml
document:
  dsl: '1.0.0'
  name: greeter
  namespace: default
  version: '0.1.0'
do:
  greet:
    set:
      greetings: '${ "Hello \(.user.firstName) \(.user.lastName)!" }'
```

Next, run the following command to create the workflow on the API:

```bash
synctl workflow create -f greeter.yaml 
```

Finally, run the following command to run the workflow with the specified JSON input:

```bash
synctl workflow run greeter --namespace default --version 0.1.0 --input '{\"user\":{\"firstName\":\"John\",\"lastName\":\"Doe\"}}'
```

The command above will provide the fully qualified name of the created workflow instance. You can utilize this name to inspect its output once the execution is finished, as demonstrated below:

```bash
synctl workflow-instance get-output greeter-uk58h3dssqp620a --namespace default --output yaml
```

For more information about `synctl`, please refer to the [documentation](#synctl).

## Community

We have a growing community working together to build a community-driven and vendor-neutral
workflow ecosystem. Community contributions are welcome and much needed to foster project growth.

See [here](community/contributors.md) for the list of community members that have contributed to the specification.

To learn how to contribute to the specification reference the ['how to contribute'](contributing.md) doc.

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

See our full project Code of Conduct information [here](code-of-conduct.md).
