# Loial
Simple asp.net 5 Github webhook continuous deployment

## Process execution
The AppPool will need to be using the identity of a user which has the rights to run the build batch file.

## Git Authorization
One way to do authorization on Windows is to use a %HOME%\\_netrc file with contents like this:

```
machine github.com
    login <user>
    password <password>
```

# Publish
```
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
dnvm use -version 1.0.0-beta7 -r clr -a x64
dnu publish --runtime active -o ..\bin\
```

LINKS
https://developer.github.com/webhooks/
http://stackoverflow.com/questions/6565357/git-push-requires-username-and-password
https://www.microsoft.com/en-us/download/confirmation.aspx?id=42637

TODO
* Slack integration
* dynamic view console output
* Make Projects path configurable
* Use Hangfire as background build processor
    * signalr refresh project list status
    * build failed or not
