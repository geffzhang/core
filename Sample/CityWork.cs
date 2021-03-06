using Greatbone.Core;

namespace Greatbone.Sample
{
    public abstract class CityWork<V> : Work where V : CityVarWork
    {
        protected CityWork(WorkContext wc) : base(wc)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class SprCityWork : CityWork<SprCityVarWork>
    {
        public SprCityWork(WorkContext wc) : base(wc)
        {
            CreateVar<SprCityVarWork, string>((prin) => ((User) prin).sprat);
        }
    }


    [Ui("城市")]
    public class AdmCityWork : CityWork<SprCityVarWork>
    {
        public AdmCityWork(WorkContext wc) : base(wc)
        {
        }

        public void @default(ActionContext ac, int page)
        {
            using (var dc = ac.NewDbContext())
            {
                dc.Sql("SELECT ").columnlst(Shop.Empty)._("FROM shops ORDER BY id LIMIT 30 OFFSET @1");
                if (dc.Query(p => p.Set(page)))
                {
                    ac.GiveGridPage(200, dc.ToArray<Shop>());
                }
                else
                {
                    ac.GiveGridPage(200, (Shop[]) null);
                }
            }
        }
    }
}