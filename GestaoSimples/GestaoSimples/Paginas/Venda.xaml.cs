using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using GestaoSimples.Servicos;
using GestaoSimples.Modelos;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GestaoSimples.Paginas
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Venda : Page, INotifyPropertyChanged
    {
        private double _valorTotal;

        private readonly ServiceVenda _servicoVenda;
        private readonly ServiceProduto _servicoProduto;

        private List<Modelos.Produto> listaProdutos { get; set; }
        private List<Modelos.ItemVenda> listaVenda { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public double ValorTotal
        {
            get { return _valorTotal; }
            set
            {
                _valorTotal = value;

                OnPropertyChanged(nameof(ValorTotal));
            }
        }

        public Venda()
        {
            this.InitializeComponent();

            _servicoVenda = new ServiceVenda();
            _servicoProduto = new ServiceProduto();

            var produtos = _servicoProduto.BuscarProdutos();
            listaProdutos = produtos;
            ProdutosListView.ItemsSource = produtos;

            NenhumProduto.Visibility = Visibility.Visible;
        }

        private async void botaoAdicionar_Click(object sender, RoutedEventArgs e)
        {
            var resultado = await ConfirmarVendaDialog.ShowAsync();

            if (resultado == ContentDialogResult.Primary)
            {
                //FinalizarVenda();
            }
            else
            {
                LimparVenda();
            }
        }

        private void FinalizarVenda()
        {
            Modelos.Venda venda = new Modelos.Venda
            {
                DataVenda = DateTime.Now,
                ItensVenda = listaVenda,
                ValorTotal = _valorTotal
            };

            _servicoVenda.AdicionarVenda(venda);
        }

        private void LimparVenda()
        {
            listaVenda.Clear();
            ValorTotal = 0;
        }

        private void Buscando(object sender, TextChangedEventArgs e)
        {
            string textoBuscado = Textobusca.Text.ToLower();
            var listaFiltrada = listaProdutos.Where(f => f.Nome.ToLower().Contains(textoBuscado) ||
                                                       f.Descricao.ToLower().Contains(textoBuscado) ||
                                                       f.Categoria.ToString().ToLower().Contains(textoBuscado)).ToList();

            ProdutosListView.ItemsSource = listaFiltrada;
        }

        private void AtualizarValorTotal()
        {
            ValorTotal = listaVenda.Sum(item => item.ValorTotalItem);
        }

        private void ClickItemLista(object sender, ItemClickEventArgs e)
        {
            NenhumProduto.Visibility = Visibility.Collapsed;

            Modelos.Produto produtoSelecionado = (Modelos.Produto)e.ClickedItem;

            if (listaVenda == null)
            {
                listaVenda = new List<ItemVenda>();
            }

            var itemVendaExistente = listaVenda.FirstOrDefault(i => i.Produto.Id == produtoSelecionado.Id);

            if (itemVendaExistente != null)
            {
                itemVendaExistente.Quantidade++;
                itemVendaExistente.ValorTotalItem = itemVendaExistente.Quantidade * itemVendaExistente.Produto.Preco;
            }
            else
            {
                var novoItemVenda = new ItemVenda
                {
                    Produto = produtoSelecionado,
                    Quantidade = 1,
                    ValorTotalItem = produtoSelecionado.Preco
                };

                listaVenda.Add(novoItemVenda);
            }

            ItensVendaListView.ItemsSource = null;
            ItensVendaListView.ItemsSource = listaVenda;

            AtualizarValorTotal();
        }

        private void ConfirmarVenda_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FinalizarVenda();
        }

        private void CancelarVenda_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LimparVenda();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
