namespace ExtensionsAI.labs.Common
{
    internal abstract class RunnerBase: IRunner
    {
        protected readonly AppConfigurations _appConfigurations;
        protected RunnerBase(AppConfigurations appConfigurations)
        {
            _appConfigurations = appConfigurations;
        }
        public abstract Task Run();
    }
}
