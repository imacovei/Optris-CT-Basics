# Some basic code samples to help starting working with the Optris CT devices
---
## Table of Contents
- [Aim](#aim)
- [Some technical aspects](#some-technical-aspects)
- [Projects](#projects)
  - [OptrisCT](#optrisct)
    - [Target framework:](#target-framework)
    - [External dependencies:](#external-dependencies)
    - [Implemented features:](#implemented-features)
      - [READ Operations](#read-operations)
      - [WRITE Operations](#write-operations)
  - [OptrisCT.cmd](#optrisctcmd)
    - [Target framework:](#target-framework-1)
    - [External dependencies:](#external-dependencies-1)
    - [Implemented features:](#implemented-features-1)

# Aim
This project aims to help starting working with Optris CT devices  
It provides some basic examples of reading and writing operations  

# Some technical aspects
The Optris CT devices can be connected over RS232 (one single device)  or over RS485 (multiple devices)  
The connectivity over RS485 allows communicating with several devices over the same serial port.  
If the devices are connected over RS485 then we need to specify with which device we want to communicat to.  
This can be achieved by specifying the address of the peer device when sending the command.  

# Projects

## OptrisCT
Implements a manger class to perform operation on one or many Optris CT devices  

### Target framework:
.net 5.0

### External dependencies:
none

### Implemented features:
#### READ Operations
* read the temperature from one single device (monitor it over a specified amount of time)
* read many temperaturea in the so called line-mode (monitor it over a specified amount of time)
* read the serial number of the device
* read the firmware version of the device
* read the emissivity

#### WRITE Operations
* set emissivity


## OptrisCT.cmd

### Target framework:  
.net 5.0

### External dependencies:  
* CommandLineParser
* Newtonsoft.Json

### Implemented features:
Accepts command line options for setting the 
* port number
* operation to be performed on the device. It may be
  * read temperature
  * read serial number
  * read firmware version
  * read emissivity
  * write emissivity
* duration
* peer address (for accessing one or many devices devices over the RS485 bus)
* value to be written, if applicable


