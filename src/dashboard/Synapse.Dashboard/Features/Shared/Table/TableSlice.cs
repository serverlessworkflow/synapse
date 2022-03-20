using Neuroglia.Data.Flux;

namespace Synapse.Dashboard
{
    public class DataTableState<T>
    {

        public List<T> ItemsSource { get; }

        public List<Column<T>> Columns { get; }

    }

    public class AddItemToList<T>
    {

        public AddItemToList(T item)
        {
            this.Item = item;
        }

        public T Item { get; }

    }

    public static class DataTableReducers
    {
        public static DataTableState<T> AddItemToList<T>(DataTableState<T> state, AddItemToList<T> action)
        {
            state.ItemsSource.Add(action.Item);
            return state;
        }

    }

}
