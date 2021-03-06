using Greatbone.Core;

namespace Greatbone.Sample
{
    public abstract class ChatWork<V> : Work where V : ChatVarWork
    {
        protected ChatWork(WorkContext wc) : base(wc)
        {
        }
    }

    [Ui("客服")]
    [User(User.AID)]
    public class OprChatWork : ChatWork<OprChatVarWork>
    {
        public OprChatWork(WorkContext wc) : base(wc)
        {
            CreateVar<OprChatVarWork, string>((prin) => ((Chat) prin).wx);
        }

        public void @default(ActionContext ac)
        {
            string shopid = ac[typeof(ShopVarWork)];
            using (var dc = ac.NewDbContext())
            {
                if (dc.Query("SELECT * FROM chats WHERE shopid = @1", p => p.Set(shopid)))
                {
                    ac.GiveGridPage(200, dc.ToArray<Chat>());
                }
                else
                {
                    ac.GiveGridPage(200, (Chat[]) null);
                }
            }
        }
    }
}