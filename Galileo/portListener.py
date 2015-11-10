#!/usr/bin/env python

import serial ## Load the serial library

## Select and configure the port
myPort = serial.Serial('/dev/cu.usbmodem1411', 115200, timeout = 10)

## Wait for data to come in- one byte, only
while(1):
	x = myPort.read()

## Echo the data to the command prompt
	print "You entered " + x

## Close the port so other applications can use it.
myPort.close()
