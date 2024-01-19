using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
    /// <summary>
    /// Interaction logic for prelevementHyptcUI.xaml
    /// </summary>
    public partial class prelevementHyptcUI : Window
    {
        PP1Entities PP1Entities = new PP1Entities();
        bool ouvert = false;
        int montant;//pour sauver le montant
        compteHptcItem selectedCompte;//pour sauver le compte selectionne
        const string typeTransaction = "paiement";
        public class compteHptcItem//objet client pour  cmbCodeClient
        {
            public Compte compte { get; set; }
            public string FullName { get; set; }
        }
        public prelevementHyptcUI()
        {
            InitializeComponent();
            try
            {
                ouvert = true;//fenetre ouverte
                var compteListe = PP1Entities.Comptes.Where(compte => compte.TypeCompte == "hypotheque").ToList();//recupere les comptes hypotheque
                List<compteHptcItem> compteHptcItem = compteListe.Select(compte => new compteHptcItem //creer object pout comboxclient
                {
                    compte = compte,//objet compte
                    FullName = PP1Entities.Clients.Where(client => client.CodeClient == compte.CodeClient).Select
                                    (client => string.Concat(client.Prenom, " ", client.Nom)).FirstOrDefault(),//string memberpath

                }).ToList();
                ComboBoxTypeCompte.ItemsSource = compteHptcItem;//etablie la source
                ComboBoxTypeCompte.DisplayMemberPath = "FullName";//affiche le nom du client
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void ComboBoxTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCompte = ComboBoxTypeCompte.SelectedItem as compteHptcItem;//recupere l'objet selectionne
            soldeCompte.Content = string.Format("{0:C2}", selectedCompte.compte.Solde);//affiche le solde du compte
        }

        private void btnPrelevement_Click(object sender, RoutedEventArgs e)
        {
            montant = Math.Abs(montant);//valeur positive 
            if (selectedCompte.compte.Solde - montant < 0)//si retrait superieur aux fonds suffisants
            {
                try
                {
                    var compteCredit = PP1Entities.Comptes
                        .Where(compte => compte.CodeClient == selectedCompte.compte.CodeClient && compte.TypeCompte == "credit").FirstOrDefault();//recupere le compte credit

                    if (compteCredit != null)//si compte credit existe
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Fond insuffisant, prendre de marge de credit?", "Confirmation", MessageBoxButton.YesNo);//affiche message

                        if (result == MessageBoxResult.Yes)//Si reponse positive
                        {
                            var difference = -1 * (selectedCompte.compte.Solde - montant);//recupere la difference manquante
                            var fondSuffisant = montant - difference;//recupere les fonds disponible
                            selectedCompte.compte.Solde -= fondSuffisant;//retire les fonds suffisant au compte selectionne
                            compteCredit.Solde += difference;//ajoute le reste a la marge de credit

                            //nouvelle transaction compte credit
                            var newCreditTransaction = new Transaction
                            {
                                CodeClient = selectedCompte.compte.CodeClient,
                                TypeTransaction = typeTransaction,
                                De = compteCredit.CodeCompte,
                                À = null,// null car prelevement
                                TransactionDate = DateTime.Now,
                                Montant = difference//difference prise dans credit
                            };
                            //nouvelle transaction compte hypotheque
                            var newTransaction = new Transaction
                            {
                                CodeClient = selectedCompte.compte.CodeClient,
                                TypeTransaction = typeTransaction,
                                De = selectedCompte.compte.CodeCompte,
                                À = null,// null car prelevement
                                TransactionDate = DateTime.Now,
                                Montant = fondSuffisant//fond suffisant pris dans compte selectionne
                            };
                           
                            PP1Entities.Transactions.Add(newTransaction);//Ajoute la transaction au compte hypotheque
                            PP1Entities.Transactions.Add(newCreditTransaction);//Ajoute la transaction au compte credit
                            System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete retire au compte {selectedCompte.compte.CodeCompte} " +
                                $"avec credit de {string.Format("{0:C2}", difference)}$");//message de confirmation
                        }
                        else//Si reponse negative
                        {
                            //rien faire
                        }
                    }
                    else//si compte credit n'existe pas
                    {
                        System.Windows.MessageBox.Show(" Transaction annulee, fond insufisant");//affiche message d'erreur
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);//affiche message erreur
                }
                
            }
            else//fond suffisant
            {
                try
                {
                    selectedCompte.compte.Solde -= montant;//preleve les fonds du compte
                    MessageBox.Show($"Un montant de {montant}$ a ete preleve du compte {selectedCompte.compte.CodeCompte}");
                    var newTransaction = new Transaction
                    {
                        CodeClient = selectedCompte.compte.CodeClient,
                        TypeTransaction = typeTransaction,
                        De = selectedCompte.compte.CodeCompte,
                        À = null,// null car retrait
                        TransactionDate = DateTime.Now,
                        Montant = montant
                    };
                    System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete retire du compte {selectedCompte.compte.CodeCompte}");//message de confirmation
                    PP1Entities.Transactions.Add(newTransaction);//ajoute la transaction
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);//affiche message d'erreur
                }
            }
            PP1Entities.SaveChanges();//sauve les changements
            this.Close();//ferme la fenetre
        }

        private void txtBoxMontantTransfert_TextChanged(object sender, TextChangedEventArgs e)
        {
            
           var montantValid = int.TryParse(txtBoxMontantTransfert.Text, out montant);//verification input numerique
            if (montantValid==false)//si montant n'et pas numerique
            {
                messageErreur.Content = "svp entrer une valeur numerique";
                btnPrelevement.IsEnabled = false;//desactive le bouton
            }
            else
            {
                messageErreur.Content = String.Empty;//vide message erreur
                btnPrelevement.IsEnabled = true;//active le bouton
            }
            
        }
    }
}
