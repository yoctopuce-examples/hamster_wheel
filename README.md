# Hamster Wheel 

## Intro
Hamster Wheel is a .NET Core service that use Yoctopuce devices to compute and display the speed of a hamseter wheel. For more detail on this application read our blog post on http://www.yoctopuce.com

## Build and debug the application

This app is compatible with .NET Core 2.1. You can build the project using Visual Studio or the ''dotnet'' command.

The command line to debug the application:
```
dotnet run
```

## Install it on a Raspberry Pi

The command line to build a self contained application that can un on Raspberry Pi (Pi2, Pi3 or Pi4):
```
dotnet publish -o hamster_wheel_pi  --self-contained -r linux-arm
```

This command create a ``hamster_wheel_pi`` directory with all the required files (including the .Net virtual machine). You must then copy this folder on the Raspberry Pi and set the execution flags to the ``HamsterWeel`` file inside this directory:
```
chmod +x HamsterWheel
```

## Install it as a service

Create a file named ``hamsterwheel.service`` in the ``/etc/systemd/system/`` directory with the following content:

```
[Unit]
Description=Yoctopuce Hamster Wheel
[Service]
Type=simple
ExecStart=/home/pi/hamster_wheel_pi/Hamster --diameter 150
[Install]
WantedBy=multi-user.target
```
*Note": that you may need to change the line that start with ``ExecStart`` with the correct parameter for your wheel.


Then you need to enable the service and start it:

```
sudo systemctl enable hamsterwheel.service
sudo systemctl start hamsterwheel.service
```

