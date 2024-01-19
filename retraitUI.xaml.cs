using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
    /// <summary>
    /// Interaction logic for retraitUI.xaml
    /// </summary>
    public partial class retraitUI : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        Compte selectedCompte;//sauve le compte choisi dans comboBox
        Client Client;//sauve le client 
        Guichet Guichet;//sauve le guichet
        const string typeTransaction = "retrait";// variable pour type de transaction
        private Window1 parentWindow;//sauve la fenetre mere
        int montant;//pour sauver le montant entre
        public retraitUI(Client client, Guichet guichet, Window1 window)
        {
            InitializeComponent();
            try
            {
                var clientAccounts = PP1Entities.Comptes
                    .Where(compte => compte.CodeClient == client.CodeClient && compte.TypeCompte == "checque"
                                | compte.TypeCompte == "epargne")
                                        .ToList();//recupere les comptes checque et epargne du clients
                ComboBoxTypeCompte.DataContext = clientAccounts;// etablie le data context
                Client = client;//sauve le client pour les fenetres transactions
                Guichet = guichet;//sauve le guichet pour les fenetres tranactions
                parentWindow = window; //sauve la fenetre mere
                multiple10Erreur.Visibility = Visibility.Hidden;// cache le message d'erreur multiple 10
                btnRetrait.IsEnabled = false;//desactive le bouton retrait
                ouvert = true; //fenetre ouverte
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ComboBoxTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)//selection de compte dans combo box
        {
            selectedCompte = ComboBoxTypeCompte.SelectedItem as Compte;//recupere le compte selectionne
            soldeCompte.Content = selectedCompte.Solde.ToString();//affiche le solde du compte selectionne
        }

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)//changement de text dand zone de saisie
        {
            var montantValid = int.TryParse(txtBoxMontantRetrait.Text, out montant);// verifie si montant est un chiffre
            if (montantValid)//si le montant entre est un chiffre
            {
                if (montant % 10 == 0)//test multiple de 10
                {
                    multiple10Erreur.Visibility = Visibility.Hidden;//cache message sur UI
                    if(montant >1000)//si le montant est superieur a 1000
                    {
                        multiple10Erreur.Content = "Impossible de retirer plus de 1000$";//adapte message
                        multiple10Erreur.Visibility = Visibility.Visible;//affiche message sur UI
                    }
                    else
                    {
                        btnRetrait.IsEnabled = true;//active le bouton retrait
                    }
                   
                }
                else
                {
                    multiple10Erreur.Content = "svp entrer un multiple de 10";//adapte message
                    multiple10Erreur.Visibility = Visibility.Visible;//affiche message sur UI
                    btnRetrait.IsEnabled = false;//desactive le bouton retrait
                }
            }
            else//sinon
            {
                multiple10Erreur.Content = "svp entrer des valeur numerique";//affiche message d'erreur.
                multiple10Erreur.Visibility = Visibility.Visible;//affiche message sur UI
                btnRetrait.IsEnabled = false;//desactive le bouton retrait
            }
            if (txtBoxMontantRetrait.Text == String.Empty)//si champ vide
            {
                multiple10Erreur.Visibility = Visibility.Hidden;//cache message sur UI
                btnRetrait.IsEnabled = false;//desactive le bouton retrait
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if(Guichet.Solde - montant < 0)//si solde du guichet insuffisant
            {
                System.Windows.MessageBox.Show(" Transaction annulee, solde du guichet insufisant");//affiche message d'erreur
            }
            else
            {
                if(selectedCompte.Solde - montant < 0)//si retrait superieur aux fonds suffisants
                {
                    try
                    {
                        var compteCredit = PP1Entities.Comptes
                            .Where(compte => compte.CodeClient == Client.CodeClient && compte.TypeCompte == "credit").FirstOrDefault();//recupere le compte credit

                        if (compteCredit != null)//si compte credit existe
                        {
                            MessageBoxResult result = System.Windows.MessageBox.Show("Fond insuffisant, prendre de marge de credit?", "Confirmation", MessageBoxButton.YesNo);//affiche message

                            if (result == MessageBoxResult.Yes)//Si reponse positive
                            {
                                var difference = -1 * (selectedCompte.Solde - montant);//recupere la difference manquante
                                var fondSuffisant = montant - difference;//recupere les fonds disponible
                                selectedCompte.Solde -= fondSuffisant;//retire les fonds suffisant au compte selectionne
                                compteCredit.Solde += difference;//ajoute le reste a la marge de credit
                                
                                //nouvelle transaction compte credit
                                var newCreditTransaction = new Transaction
                                {
                                    CodeClient = Client.CodeClient,
                                    TypeTransaction = typeTransaction,
                                    De = compteCredit.CodeCompte,
                                    À = null,// null car retrait
                                    TransactionDate = DateTime.Now,
                                    Montant = difference//difference prise dans credit
                                };
                                //nouvelle transaction compte selectionne
                                var newTransaction = new Transaction
                                {
                                    CodeClient = Client.CodeClient,
                                    TypeTransaction = typeTransaction,
                                    De = selectedCompte.CodeCompte,
                                    À = null,// null car retrait
                                    TransactionDate = DateTime.Now,
                                    Montant = fondSuffisant//fond suffisant pris dans compte selectionne
                                };
                                
                                    PP1Entities.Transactions.Add(newTransaction);//Ajoute la transaction au compte selectionne
                                   
                                PP1Entities.Transactions.Add(newCreditTransaction);//Ajoute la transaction au compte credit
                                System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete retire au compte {selectedCompte.CodeCompte} avec credit de {string.Format("{0:C2}", difference)}$");//message de confirmation
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
                    }catch(Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);//affiche message erreur
                    }
                    finally
                    {
                        PP1Entities.Guichets.Where(guichet => guichet.CodeGuichet == Guichet.CodeGuichet).FirstOrDefault().Solde -= montant;//retire le montant au guichet;
                        PP1Entities.SaveChanges();//sauve les changement 
                    }
                }
                else//fond suffisant
                {
                    try
                    {
                        selectedCompte.Solde -= montant;//retire les fonds du compte selectionne
                        Guichet.Solde -= montant;//retire le montant au guichet;
                        var newTransaction = new Transaction
                        {
                            CodeClient = Client.CodeClient,
                            TypeTransaction = typeTransaction,
                            De = selectedCompte.CodeCompte,
                            À = null,// null car retrait
                            TransactionDate = DateTime.Now,
                            Montant = montant
                        };
                        System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete retire du compte {selectedCompte.CodeCompte}");//message de confirmation
                        PP1Entities.Guichets.Where(guichet => guichet.CodeGuichet == Guichet.CodeGuichet).FirstOrDefault().Solde -= montant;//retire le montant au guichet;
                        PP1Entities.SaveChanges();//sauve les changements
                    }catch(Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);//affiche message d'erreur
                    }
                }
            }
            
            parentWindow.RefreshUIElements(); //recharge l'UI avec les nouvelles donnees
            ouvert = false;//fenetre fermee
            this.Close();//ferme la fenetre depot
        }
    }
}
