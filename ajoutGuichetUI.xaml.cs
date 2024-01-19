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
    /// Interaction logic for ajoutGuichetUI.xaml
    /// </summary>
    public partial class ajoutGuichetUI : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        Guichet selectedGuichet;//pour sauver le guichet selectionne
        public ajoutGuichetUI()
        {
            InitializeComponent();
            try
            {
                ouvert = true;//fenetre ouverte
                var listeguichets = PP1Entities.Guichets.ToList();//recupete les guichets
                cmbGuichet.ItemsSource = listeguichets;
                cmbGuichet.DisplayMemberPath = "CodeGuichet";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void txtBoxMontant_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void cmbGuichet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedGuichet = cmbGuichet.SelectedItem as Guichet;//recupere le guichet selectionne
            soldeGuichet.Content = string.Format("{0:C2}", selectedGuichet.Solde);//change le solde en fonction du guichet
        }

        private void btnAjout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var montant = 0;//pour sauver le montant
                var isMontantInt = int.TryParse(txtBoxMontant.Text, out montant);//converti en int
                int limite = (int)(selectedGuichet.Solde + montant);//sauve la somme du solde et montant
                if (isMontantInt)//si convertion reussie
                {
                    if (limite > 20000)//limite d'argent de 20 000$
                    {
                        var soldeCorrecte = -1 * (20000 - limite);//trouve le surplus
                        montant -= soldeCorrecte;//retire le surplus
                        selectedGuichet.Solde += montant;//ajoute le montant modifie
                        MessageBox.Show($"Pour des raison de securite, seulement {montant}$ on ete ajoute au guichet {selectedGuichet.CodeGuichet} en raison de la limite de 20 000$");//previent de modification
                    }
                    else//limite non depasse
                    {
                        selectedGuichet.Solde += montant;//ajoute les fonds au guichet
                        MessageBox.Show($"Un montant de {montant}$ a ete ajoute au guichet {selectedGuichet.CodeGuichet}");//message de confirmation
                    }
                }
                else//sinon
                {
                    MessageBox.Show("svp entrer une valeur numerique");//message erreur
                }
                PP1Entities.SaveChanges();//sauve les changements
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
