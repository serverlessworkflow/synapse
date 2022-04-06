namespace Synapse.Dashboard
{
    public class RowRenderingContext<T>
    {

        public RowRenderingContext(Table<T> table, T item, int index)
        {
            this.Table = table;
            this.Item = item;
            this.Index = index;
        }

        public Table<T> Table { get; }

        public T Item { get; }

        public int Index { get; }

    }

}
