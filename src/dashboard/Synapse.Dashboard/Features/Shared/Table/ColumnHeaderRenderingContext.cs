namespace Synapse.Dashboard
{
    public class ColumnHeaderRenderingContext<T>
    {

        public ColumnHeaderRenderingContext(Table<T> table, Column<T> column)
        {
            this.Table = table;
            this.Column = column;
        }

        public Table<T> Table { get; }

        public Column<T> Column { get; }

    }

}
