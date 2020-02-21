<hr/>
<h1 align="center">
	CDN.NET
</h1>
<h2 align="center">
Simple yet powerful file hosting service
</h2>
<hr/>
<p align="center">
    This is the backend portion including the API wrapper
    <br>
	Join this Discord Server for support (It's originally for another app but im generally there).
	<br>
	<br>
    <a href="https://discord.gg/Pah4yj5">
        <img src="https://discordapp.com/api/guilds/281589163659362305/widget.png?style=banner2">
    </a>
</p>
<hr/>

[![NuGet](https://img.shields.io/nuget/vpre/CDN.NET.Wrapper.svg?maxAge=2592000?style=flat)](https://www.nuget.org/packages/CDN.NET.Wrapper)

## What is CDN.NET

CDN.NET is an easy to setup and use file storage and hosting system. Thanks to its highly parallel asp.net core backend CDN.NET is extremely fast and responsive. The config files make it insanely easy to configure the service to your liking, supporting MySql or MariaDB for its database, private and public hosting mode as well as custom banned extensions and more!

## CDN.NET is still under heavy construction
It is supposed to be the backbone of a future startup of mine and all other public hosting needs i may need in the future like Sora's own images. Thus this project will be heavily optimized for thousands of users and in the further future should be an actual CDN with multiple nodes.

## Frontend
![Frontend](https://i.imgur.com/aF6284F.png)
[You can find the Frontend here](https://github.com/Daniele122898/CDN-NET-frontend)

## Setup backend for self hosting

## Config
Checkout the example appsettings.json.example file to see what you will need to configure yourself. 

### Some Values Explained:
#### Token
This will be the JWT secret. Make sure this is a very long and secure secret!
#### Private
If set to true, the entire regristration endpoint is disabled. (Create your own account in public first then switch to private)
#### MaxSize
The maximum size in Mb a file is allowed to be.
#### UploadSettings:MaxFiles
The maximum amount of files a user can upload at once using the multi endpoint.
#### LimitSettings:MaxFiles
The maximum amount of files a user is allowed to have uploaded at any time
#### MaxAlbums
The maximum amount of albums a user is allowed to have at any time.

## Databse

I personally use and test on MariaDb which i also recommend using. Simply install it and setup on whatever operating system you are on. 

#### For Windows

Start your local mariadb instance
```
net start mariadb
```

Stop your local mariadb instance
```
net stop mariadb
```
If you want to disable the auto start of the mariadb windows service do the following:
Windows + R `services.msc` and set MariaDB to start manually

## Linux Troubleshooting

### Can't run Wyam

Try this if wyam cant be found
`export PATH="$PATH:/home/argonaut/.dotnet/tools"`

Try this if there's an error about libhostfxr.so
`export DOTNET_ROOT="/opt/dotnet"`