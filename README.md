# PANSearcher
![Build](https://github.com/zbalkan/PANSearcher/actions/workflows/dotnet.yml/badge.svg)

## Description

A command line application to search for PAN numbers in files. It is a hobby project and not ready for production use. The idea is to be a drop-in replacement for PANHunt on all operating systems.

Although it is written from scratch, it is using directly (ripping off) some parts of PANHunt such as Regex patterns.

## Usage

```
usage: PANSearcher [ARGUMENTS]
```
### Arguments

| Short | Long | Description | Default Value | Status |
|-------|------|-------------|---------------|--------|
| -h | --help | Show this help message and exit | | IMPLEMENTED |
| -s | --search | Base directory to search in | Windows: C:\, Others: / | IMPLEMENTED |
| -x | --exclude | Directories to exclude from the search | Windows: C:\Windows,C:\Program Files,C:\Program Files (x86). Others: /mnt | IMPLEMENTED |
| -t | --TEXTFILES | Text file extensions to search | .doc,.xls,.xml,.txt,.csv | NOT IMPLEMENTED |
| -z | --ZIPFILES | zip file extensions to search | .docx,.xlsx,.zip | NOT IMPLEMENTED |
| -e | --SPECIALFILES | Special file extensions to search | .msg | NOT IMPLEMENTED |
| -m | --MAILFILES | Email file extensions to search | .pst | NOT IMPLEMENTED |
| -l | --OTHERFILES | Other file extensions to list | .ost,.accdb,.mdb | NOT IMPLEMENTED |
| -o | --OUTFILE | Output file name for PAN report | panhunt_YYYY-MM-DD-HHMMSS.txt | NOT IMPLEMENTED |
| -C | --config | configuration file to use | | IMPLEMENTED |
| -X | --EXCLUDEPAN | The PAN to exclude from search |   | NOT IMPLEMENTED |
| -u | --unmask | Unmask PANs in output | | IMPLEMENTED |
| -t | --truncate | Truncate PANs in output | | IMPLEMENTED |
| -q | --quiet | Quiet  | | NOT IMPLEMENTED |
| -f | --format | Format report. Acceptable values: txt, xml, json. | txt  | NOT IMPLEMENTED |


## Dependencies
- dotnet 6.0

## Installation
Currently it is portable. Just compile and run.

## TODO
- [x] Get rid of interactive UI and use SYSTEM drive as default search root
- [x] Use the configuration that PANHunt presents
- [ ] Use the same flags with PANHunt
- [ ] Add Quiet option
- [ ] Add report ormat option
- [ ] Add Word, Excel, PST and MSG file reading capability
- [ ] Add advanced logging options such as json, syslog, event log formats pushing to a syslog server
- [ ] Add email alerts