using Microsoft.Extensions.Configuration;

namespace Coder.Apollo.Sdk
{
    /// <summary>
    /// apollo配置数据源
    /// </summary>
    public class ApolloConfigurationSource : IConfigurationSource
    {
        private readonly ApolloOptions _apolloOptions;

        public ApolloConfigurationSource(ApolloOptions apolloOptions)
        {
            _apolloOptions = apolloOptions;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ApolloConfigurationProvider(_apolloOptions);
        }
    }
}