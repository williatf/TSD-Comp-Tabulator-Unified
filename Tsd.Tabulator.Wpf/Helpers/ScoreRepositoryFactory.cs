using System;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Data.Sqlite;
using Tsd.Tabulator.Data.Sqlite.Scoring;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Helpers
{
    public sealed class ScoreRepositoryFactory : IScoreRepositoryFactory
    {
        private readonly ShellViewModel _shell;

        public ScoreRepositoryFactory(ShellViewModel shell)
        {
            _shell = shell ?? throw new ArgumentNullException(nameof(shell));
        }

        public IScoreRepository Create()
        {
            if (!_shell.HasEventLoaded || string.IsNullOrWhiteSpace(_shell.CurrentDbPath))
                throw new InvalidOperationException("No event is open. Cannot create ScoreRepository.");

            var factory = new SqliteConnectionFactory(_shell.CurrentDbPath);
            return new ScoreRepository(factory);
        }
    }
}
