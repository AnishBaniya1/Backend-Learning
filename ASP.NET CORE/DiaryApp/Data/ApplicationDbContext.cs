using Microsoft.EntityFrameworkCore;

namespace DiaryApp.Data
{
    //iherit from DbContext which is part of EF CORE
    public class ApplicationDbContext : DbContext
    {
        //in constructor pass DbContextOptions of type<ApplicationDbContext>
        // it will pass the options to base class dbcontext and own class applicationdbcontext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

    }
}