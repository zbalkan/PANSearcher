# PANSearcher
![Build](https://github.com/zbalkan/PANSearcher/actions/workflows/dotnet.yml/badge.svg)

## Description

A command line application to search for PAN numbers in files. It is a hobby project and not ready for production use. The idea is to be a drop-in replacement for PANHunt on all operating systems.

Although it is written from scratch, it is using directly (ripping off) some parts of PANHunt such as Regex patterns.

## Usage

```
usage: PANSearcher [-h] [-s SEARCH] [-x EXCLUDE] [-t TEXTFILES] [-z ZIPFILES] [-e SPECIALFILES] [-m MAILFILES] [-l OTHERFILES] [-o OUTFILE] [-u] [-t]

PANSearcher: search directories and sub directories for documents containing PANs.

optional arguments:
  -h, --help       show this help message and exit
  -s search        base directory to search in (default: C:\)
  -x exclude       directories to exclude from the search (default: C:\Windows,C:\Program Files,C:\Program Files (x86))
  -t TEXTFILES     text file extensions to search (default: .doc,.xls,.xml,.txt,.csv) [NOT IMPLEMENTED]
  -z ZIPFILES      zip file extensions to search (default: .docx,.xlsx,.zip) [NOT IMPLEMENTED]
  -e SPECIALFILES  special file extensions to search (default: .msg) [NOT IMPLEMENTED]
  -m MAILFILES     email file extensions to search (default: .pst) [NOT IMPLEMENTED]
  -l OTHERFILES    other file extensions to list (default: .ost,.accdb,.mdb) [NOT IMPLEMENTED]
  -o OUTFILE       output file name for PAN report (default: panhunt_YYYY-MM-DD-HHMMSS.txt) [NOT IMPLEMENTED]
  -C config        configuration file to use
  -X EXCLUDEPAN    the PAN to exclude from search [NOT IMPLEMENTED]
  -u               unmask PANs in output (default: False)
  -t               truncate PANs in output (default:False)
  -q               Quiet  [NOT IMPLEMENTED]
  -f               Format report. Acceptable values: txt (default), xml, json.  [NOT IMPLEMENTED]
```


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