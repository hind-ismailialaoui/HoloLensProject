#!/usr/bin/env python3
###### TeraRanger Evo Example Code STD #######
#                                            #
# All rights reserved Terabee France (c) 2018#
#                                            #
############ www.terabee.com #################

import serial
import serial.tools.list_ports
import sys
import crcmod.predefined
import socket  # Importer le module pour l'envoi UDP

UDP_IP = "192.168.1.100"  # Adresse IP du destinataire (ton PC)
UDP_PORT = 5005           # Port sur lequel tu envoies les messages UDP

def findEvo():
    print('Scanning all live ports on this PC')
    ports = list(serial.tools.list_ports.comports())
    for p in ports:
        if "5740" in p[2]:
            print('Evo found on port ' + p[0])
            return p[0]
    return 'NULL'

def openEvo(portname):
    print('Attempting to open port...')
    evo = serial.Serial(portname, baudrate=115200, timeout=2)
    set_bin = (0x00, 0x11, 0x02, 0x4C)
    evo.flushInput()
    evo.write(set_bin)
    evo.flushOutput()
    print('Serial port opened')
    return evo

def get_evo_range(evo_serial):
    crc8_fn = crcmod.predefined.mkPredefinedCrcFun('crc-8')
    data = evo_serial.read(1)

    if data == b'T':
        frame = data + evo_serial.read(3)
        if frame[3] == crc8_fn(frame[0:3]):
            rng = frame[1] << 8
            rng = rng | (frame[2] & 0xFF)
        else:
            return "CRC mismatch. Check connection or make sure only one program accesses the sensor port."
    else:
        return "Waiting for frame header"

    if rng == 65535:
        return float('inf')
    elif rng == 1:
        return float('nan')
    elif rng == 0:
        return -float('inf')
    else:
        return rng / 1000.0  # Retourner la distance en mètres

def send_data_udp(message, udp_socket):
    udp_socket.sendto(message.encode(), (UDP_IP, UDP_PORT))

if __name__ == "__main__":
    print('Starting Evo data streaming')
    port = findEvo()

    if port == 'NULL':
        print("Sorry, couldn't find the Evo. Exiting.")
        sys.exit()

    evo = openEvo(port)

    # Création du socket UDP pour envoyer des messages
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    try:
        while True:
            distance = get_evo_range(evo)

            if isinstance(distance, float) and distance != float('inf') and not distance != distance:
                print(f"Distance: {distance} meters")
                
                # Envoie des messages UDP avec la distance
                message = f"{distance}"
                send_data_udp(message, udp_socket)

    except serial.serialutil.SerialException:
        print("Device disconnected (or multiple access on port). Exiting...")

    evo.close()
    udp_socket.close()
    sys.exit()
