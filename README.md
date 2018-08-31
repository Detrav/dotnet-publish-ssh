# dotnet-publish-ssh

Simple publish your .Net Core application to linux server via SSH.

# Usage

* Add this to `csproj` file:
```XML
  <ItemGroup>
    <DotNetCliToolReference Include="DotnetPublishSsh" Version="0.1.0" />
  </ItemGroup>
```

* Run `dotnet restore`
* Run `dotnet publish-ssh` with options:
```
Usage: dotnet publish-ssh [arguments] [options]
Arguments and options are the same as for `dotnet publish`
SSH specific options:
  --ssh-host *              Host address
  --ssh-port                Host port
  --ssh-user *              User name
  --ssh-password            Password
  --ssh-keyfile             Private OpenSSH key file
  --ssh-path *              Publish path on remote server
(*) required
```

# Example

`dotnet publish-ssh --ssh-host 10.0.0.1 --ssh-port 22 --ssh-user root --ssh-password secret --ssh-path /var/www/site`

# TODO

- [x] Just works
- [x] Password authentication
- [x] Private key file authentication
- [ ] Don't upload unmodified files (checksum)
- [ ] Config file
- [ ] Pre/post publish hooks on remote server

# After fork changelog

```
[2018/08/30][0.1.1] added --ssh-script option for calling script after publishing
[2018/08/31][0.1.2] replace --ssh-script command to --ssh-cmd-before and --ssh-cmd-after for run command before and after publishing on remote machine via ssh
[2018/08/31][0.1.3] added console message on command calling
[2018/08/31][0.1.4] added checking for sha1 while uploading
[2018/08/31][0.1.5] hotfix
[2018/08/31][0.1.6] hotfix2
[2018/08/31][0.1.7] added cheching for created time while uploading
[2018/08/31][0.1.8] hotfix