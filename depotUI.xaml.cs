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
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        Compte selectedCompte;//sauve le compte choisi dans comboBox
        Client Client;//sauve le client 
        Guichet Guichet;//sauve le guichet
        const string typeTransaction = "depot";// variable pour type de transaction
        private Window1 parentWindow;//sauve la fenetre mere
        public Window2 (Client client, Guichet guichet, Window1 window)
        {
            InitializeComponent();
            try
            {
                var clientAccounts = PP1Entities.Comptes
                    .Where(compte => compte.CodeClient == client.CodeClient && compte.TypeCompte != "credit")
                        .ToList(); ;//recupere les comptes du clients
                ComboBoxTypeCompte.DataContext = clientAccounts;// etablie le data context
                Client = client;//sauve le client pour les fenetres transactions
                Guichet = guichet;//sauve le guicher pour les fenetres tranactions
                parentWindow = window; //sauve la fenetre mere
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ComboBoxTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCompte = ComboBoxTypeCompte.SelectedItem as Compte;//sauve le compte selectionne
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int montant;// pour sauver le string montant
            bool isMontantValid = int.TryParse(txtBoxMontantDepot.Text, out montant);//convertie en int
            if (isMontantValid)//si convertion reussie
            {
                try
                {
                    selectedCompte.Solde += montant;//ajoute le depot au solde courant
                    PP1Entities.SaveChanges();//sauvegarde les changements fait
                    var a = PP1Entities.Comptes.Where(compte => compte.CodeClient == selectedCompte.CodeClient && compte.TypeCompte == selectedCompte.TypeCompte)
                                    .Select(compte => compte.CodeCompte).FirstOrDefault();//recupere le code compte     

                    var newTransaction = new Transaction
                    {
                        CodeClient = Client.CodeClient,
                        TypeTransaction = typeTransaction,
                        De = null,// null car depot
                        À = a ,
                        TransactionDate = DateTime.Now,
                        Montant = montant
                    };
                    PP1Entities.Transactions.Add(newTransaction);//Ajoute la transaction
                   
                    PP1Entities.SaveChanges();//sauve les changements
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    parentWindow.RefreshUIElements(); //recharge l'UI avec les nouvelles donnees
                    MessageBox.Show($"Montant de { string.Format("{0:C2}", montant)}$ a ete ajoute au compte {selectedCompte.CodeCompte}");//message de confirmation
                    this.Close();//ferme la fenetre depot
                }
            }
            else//sinon
            {
                MessageBox.Show("Entrez une valeur numerique entiere pour le depot.");
            }
        }
      

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
