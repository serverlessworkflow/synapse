<p align="center">
<img src="logos/synapse-color.png" height="350px" alt="Synapse Logo"/>
</p>

# Synapse - Kubernetes-native Serverless Workflow Runtime

Kubernetes-native runtime implementation of the Serverless Workflow specification
written in .NET 5.0

## Overview

## Features

- [x] Supports Serverless Workflow specification 0.6x
- [x] Runs on Kubernetes
- [x] No software dependencies (aside from Kubernetes)
- [x] A Command Line Interface, synctl, to quickly easily manage workflows and their instances

## Usage

### 1. Download the Synapse CLI (synctl)

```shell
wget 
```

### 2. Install Synapse

```shell
synctl install -n synapse
```

### 3. Verify installation

```shell
kubectl get pods -n synapse
```
Sample output:

```shell
NAME                                READY   STATUS    RESTARTS   AGE
synapse-operator-7f9bdd899f-hd8s2   1/1     Running   0          106m
```

### 4. Create a new Serverless Workflow definition

```yaml
#petStoreLogin.yaml
id: petStoreLogin
version: '1.0'
name: Pet Store Login
description: Logs in Swagger's Pet Store API
start: login
functions:
- name: petStoreLogin
  operation: https://petstore.swagger.io/v2/swagger.json#loginUser
states:
- name: login
  type: operation
  actions:
  - name: petStoreLogin
    functionRef:
      refName: petStoreLogin
      parameters:
        username: runtime@synapse.sw
        password: FakePassword
    actionDataFilter:
      results: '${ { loginResult: .message } }'
  end: true
```

### 5. Deploy and run a new workflow instance

```shell
synctl run --file petStoreLogin.yaml --wait
```
Sample output:

```shell
Deploying workflow 'petStoreLogin:1.0'...
Workflow deployed.
Waiting for Synapse Operator feedback...
The Synapse Operator has finished processing the workflow:
Status: VALID
Errors: []
Workflow instance 'petstorelogin-q5b7z' has been executed.
Status: 'EXECUTED'
Output: {
  "loginResult": "logged in user session:1620126182003"
}
```
