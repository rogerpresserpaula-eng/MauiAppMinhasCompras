using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

    public ListaProduto()
    {
        InitializeComponent();

        lst_produtos.ItemsSource = lista;

        // NOVO
        pk_categoria.SelectedIndex = 0;
    }

    protected async override void OnAppearing()
    {
        try
        {
            lista.Clear();

            List<Produto> tmp = await App.Db.GetAll();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            Navigation.PushAsync(new Views.NovoProduto());

        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        await FiltrarProdutos();
    }

    // NOVO
    private async void pk_categoria_SelectedIndexChanged(object sender, EventArgs e)
    {
        await FiltrarProdutos();
    }

    // NOVO
    private async Task FiltrarProdutos()
    {
        try
        {
            string busca = txt_search.Text ?? "";
            string categoria = pk_categoria.SelectedItem?.ToString();

            List<Produto> tmp = await App.Db.GetAll();

            // Filtro pela descrição
            if (!string.IsNullOrWhiteSpace(busca))
            {
                tmp = tmp.Where(p =>
                    p.Descricao.Contains(
                        busca,
                        StringComparison.OrdinalIgnoreCase
                    )).ToList();
            }

            // Filtro pela categoria
            if (!string.IsNullOrEmpty(categoria) && categoria != "Todos")
            {
                tmp = tmp.Where(p => p.Categoria == categoria).ToList();
            }

            lista.Clear();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double soma = lista.Sum(i => i.Total);

        string msg = $"O total é {soma:C}";

        DisplayAlert("Total dos Produtos", msg, "OK");
    }

    // NOVO
    private async void ToolbarItem_Relatorio(object sender, EventArgs e)
    {
        try
        {
            List<Produto> produtos = await App.Db.GetAll();

            var relatorio = produtos
                .GroupBy(p => p.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(p => p.Total)
                });

            string mensagem = "";

            foreach (var item in relatorio)
            {
                mensagem += $"{item.Categoria}: {item.Total:C}\n";
            }

            if (mensagem == "")
            {
                mensagem = "Nenhum produto cadastrado.";
            }

            await DisplayAlert(
                "Gastos por Categoria",
                mensagem,
                "OK"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecinado = sender as MenuItem;

            Produto p = selecinado.BindingContext as Produto;

            bool confirm = await DisplayAlert(
                "Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;

            Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p,
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            lista.Clear();

            List<Produto> tmp = await App.Db.GetAll();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }
}