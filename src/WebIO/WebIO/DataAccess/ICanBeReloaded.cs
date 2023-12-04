namespace WebIO.DataAccess;

using Application;

public interface ICanBeReloaded
{
    void Reload(IConfigFileReader fileReader, ScriptHelper scriptHelper, bool initScripts = true);
}