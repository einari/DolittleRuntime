# Runtime

![CI/CD](https://github.com/dolittle/Runtime/workflows/.NET%20Docker%20Image%20CI/CD/badge.svg)

[![codecov](https://codecov.io/gh/dolittle/runtime/branch/master/graph/badge.svg)](https://codecov.io/gh/dolittle/runtime)

[![Docker](https://img.shields.io/docker/v/dolittle/runtime?sort=semver)](https://hub.docker.com/r/dolittle/runtime)


Dolittle is a decentralized, distributed, event-driven microservice platform built to harness the power of events.

The runtime is the backend of our system and manages connections from the SDKs and other runtimes to it's Event Store. It's called the runtime as it's what runs and powers the SDK's to do their job.

# Get Started
- Try our [tutorial](https://dolittle.io/docs/tutorials/)
- See our [documentation](https://dolittle.io)

## Packages

| Platform | Version   |
| ------- | ------- | ------ |
| Docker | [![Docker](https://img.shields.io/docker/v/dolittle/runtime?sort=semver)](https://hub.docker.com/r/dolittle/runtime)

# Developing

## Cloning

This repository has sub modules, clone it with:

```shell
$ git clone --recursive <repository url>
```

If you've already cloned it, you can get the submodules by doing the following:

```shell
$ git submodule update --init --recursive
```

## Building

All the build things are from a submodule.
To build, run one of the following:

Windows:

```shell
$ Build\build.cmd
```

Linux / macOS

```shell
$ Build\build.sh
```

## Running
To run the runtime locally:
```terminal
cd Source/Server
dotnet run
```

## Visual Studio

You can open the `.sln` file in the root of the repository and just build directly.

## VSCode

From the `Build` submdoule there is also a .vscode folder that gets a symbolic link for the root. This means you can open the
root of the repository directly in Visual Studio Code and start building. There are quite a few build tasks, so click F1 and type "Run Tasks" and select the "Tasks: Run Tasks"
option and then select the build task you want to run. It is folder sensitive and will look for the nearest `.csproj` file based on the file you have open.
If it doesn't find it, it will pick the `.sln` file instead.

## More details

To learn more about the projects of Dolittle and how to contribute, please go [here](https://github.com/dolittle/Home).

## Getting Started

Go to our [documentation site](http://www.dolittle.io) and learn more about the project and how to get started.
Samples can also be found [here](https://github.com/Dolittle-Samples).
You can find entropy projects [here](https://github.com/Dolittle-Entropy).

# Issues and Contributing
To learn how to contribute please read our [contributing](https://dolittle.io/contributing/) guide.

File issues to our [Home](https://github.com/dolittle/Home/issues) repository.
