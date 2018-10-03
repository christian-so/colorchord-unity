import socket
import sys
import serial
leds = 75
unityUDPSource = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
unityUDPSource.bind(('0.0.0.0', 5519))
unityDebug = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

arduino = serial.Serial('COM4', 115200)

print("Piping ...")
while True:
	message, source = unityUDPSource.recvfrom(3*leds)
	unityDebug.sendto(message, ('localhost', 5521))
	arduino.write(message)