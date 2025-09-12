using CineTrackBE.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineTrackBE.Tests.Helpers.TestSetups.Universal
{
    public class InMemoryDbTestSetup: IDisposable
    {
        public ApplicationDbContext Context { get; }

        private InMemoryDbTestSetup(ApplicationDbContext context)
        {
            Context = context;
        }

        public static InMemoryDbTestSetup Create()
        {
            var context = DatabaseTestHelper.CreateInMemoryContext(false);

            return new InMemoryDbTestSetup(context);
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
