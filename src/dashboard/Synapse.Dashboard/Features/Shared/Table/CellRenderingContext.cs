namespace Synapse.Dashboard
{
    public class CellRenderingContext<T>
    {

        public CellRenderingContext(Table<T> table, Column<T> column, T item)
        {
            this.Table = table;
            this.Column = column;
            this.Item = item;
        }

        public Table<T> Table { get; }

        public Column<T> Column { get; }

        public T Item { get; }

        private object? _Value;

        public object? Value
        {
            get
            {
                if (this._Value == null)
                    this._Value = this.Column.GetValueFor(this.Item);
                return this._Value;
            }
        }

    }

}
