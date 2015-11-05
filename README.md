# Loial
Simple asp.net 5 Github webhook continuous deployment

## Authorization
One way to do authorization on Windows is to use a %HOME%\_netrc file with contents like this:

```
machine github.com
       login <user>
       password <password>
```

REF: http://stackoverflow.com/questions/6565357/git-push-requires-username-and-password

# Publish
```
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
dnvm use -version 1.0.0-beta7 -r clr -a x64
dnu publish --runtime active -o ..\bin\
```
