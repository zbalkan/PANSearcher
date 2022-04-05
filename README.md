# PANSearcher
![Build](https://github.com/zbalkan/PANSearcher/actions/workflows/dotnet.yml/badge.svg)
[![DepShield Badge](https://depshield.sonatype.org/badges/zbalkan/PANSearcher/depshield.svg)](https://depshield.github.io)

## Description

A command line application to search for PAN numbers in files. It is a hobby project and not ready for production use. The idea is to be a drop-in replacement for PANHunt on all operating systems.

Although it is written from scratch, it is using directly (ripping off) some parts of PANHunt such as Regex patterns.

## Dependencies
- dotnet 6.0

## Installation
Currently it is portable. Just compile and run.

## TODO
- [ ] Get rid of interactive UI and use SYSTEM drive as default search root
- [ ] Use the same flags with PANHunt
- [ ] Add Word, Excel, PST and MSG file reading capability
- [ ] Use the configuration that PANHunt presents
- [ ] Add advanced logging options such as json, syslog, event log formats pushing to a syslog server
- [ ] Add email alerts