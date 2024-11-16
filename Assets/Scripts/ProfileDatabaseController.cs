using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using Object = System.Object;

/// <summary>
/// Database Controller for profile page
/// </summary>
public class ProfileDatabaseController : MonoBehaviour
{
    // db reference variables
    private DatabaseReference reference;
    private FirebaseAuth auth;
    private FirebaseUser user;
    
    // profile text variables
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI dateCreatedText;
    [SerializeField] private TextMeshProUGUI emailText;
    
    // screens
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject profileScreen;
    
    // function panels and text
    [SerializeField] private GameObject updateUsernamePanel;
    [SerializeField] private TextMeshProUGUI updateUsernameValidationText;
    [SerializeField] private TMP_InputField updateUsernameInputField;
    [SerializeField] private GameObject updateEmailPanel;
    [SerializeField] private TextMeshProUGUI updateEmailValidationText;
    [SerializeField] private TMP_InputField updateEmailInputField;
    [SerializeField] private GameObject updatePasswordPanel;
    [SerializeField] private TextMeshProUGUI updatePasswordValidationText;
    [SerializeField] private TMP_InputField updatePasswordInputField;
    
    public void DisplayProfile()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        if (user != null)
        {
            reference.Child("players").Child(user.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("DisplayProfile GetValueAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("DisplayProfile GetValueAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    string username = snapshot.Child("name").Value.ToString();
                    string accountCreationDate = DateTime.UnixEpoch.AddSeconds(int.Parse(snapshot.Child("creationDate").Value.ToString())).ToLocalTime().Date.ToShortDateString();
                    string email = snapshot.Child("email").Value.ToString();
                
                    usernameText.text = username;
                    dateCreatedText.text = "Account Created: " + accountCreationDate;
                    emailText.text = "Email: " + email;
                }
            });
        }
    }

    public void UpdateUsername()
    {
        updateUsernameValidationText.color = Color.red;
        string newUsername = updateUsernameInputField.text;
        
        //Update Database Username
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        childUpdates["players/" + user.UserId + "/name/"] = newUsername;
        childUpdates["leaderboard/" + user.UserId + "/name/"] = newUsername;

        reference.UpdateChildrenAsync(childUpdates);
        StartCoroutine(ClosePanel(("Your new username is " + newUsername + "!"), updateUsernameValidationText, updateUsernamePanel, false));
    }

    public void UpdateEmail()
    {
        updateEmailValidationText.color = Color.red;
        string newEmail = updateEmailInputField.text;
        
        //Update Auth Email
        if (user != null) {
            user.UpdateEmailAsync(newEmail).ContinueWithOnMainThread(task => { // if developing further change to SendEmailVerificationBeforeUpdatingEmailAsync
                if (task.IsCanceled) {
                    updateEmailValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("UpdateEmail UpdateEmailAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    updateEmailValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("Update Email UpdateEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    Debug.Log("User email updated successfully.");
                            
                    //Update Database Email
                    Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
                    childUpdates["players/" + user.UserId + "/email/"] = newEmail;

                    reference.UpdateChildrenAsync(childUpdates);
                    StartCoroutine(ClosePanel("Your new email is " + newEmail + "!", updateEmailValidationText, updateEmailPanel, false));
                }
            });
        }
    }

    public void UpdatePassword()
    {
        updatePasswordValidationText.color = Color.red;
        string newPassword = updatePasswordInputField.text;
        
        //Update Auth Password
        if (user != null) {
            user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    updateEmailValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("UpdatePasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    updatePasswordValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("UpdatePasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    Debug.Log("Password updated successfully.");
                    StartCoroutine(ClosePanel("Your password has been updated, please login again", updatePasswordValidationText, updatePasswordPanel, true));
                }
            });
        }
    }

    public void DeleteAccount()
    {
        //Delete Auth User
        if (user != null) {
            user.DeleteAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("DeleteAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User deleted successfully.");
            });
        }
        
        //Delete Database User
        reference.Child("players").Child(user.UserId).RemoveValueAsync();
        reference.Child("leaderboard").Child(user.UserId).RemoveValueAsync();
        loginScreen.SetActive(true);
        profileScreen.SetActive(false);
    }

    public void Logout()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        Debug.Log("Log Out!");
        loginScreen.SetActive(true);
        profileScreen.SetActive(false);
        if (user != null && auth != null)
        {
            auth.SignOut();
        }
        else
        {
            Debug.LogError("Cannot sign out: user or auth is null.");
        }
    }
    
    //validation&exceptions
    public string HandleAuthExceptions(System.AggregateException e)
    {
        string validationText = "";

        if (e != null)
        {
            FirebaseException firebaseEx = e.GetBaseException() as FirebaseException;

            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            Debug.LogError("Error in auth.... error code: " + errorCode);

            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    validationText += "Wrong Password";
                    break;
                case AuthError.UserNotFound:
                    validationText += "User does not exist, please create an account";
                    break;
                case AuthError.InvalidEmail:
                    validationText += "Invalid Email";
                    break;
                case AuthError.WeakPassword:
                    validationText += "Weak Password (Minimum 8 Characters, requires upper case letters, lower case letters, numbers)";
                    break;
                case AuthError.EmailAlreadyInUse:
                    validationText += "Email is already in use, try logging in";
                    break;
                case AuthError.UserMismatch:
                    validationText += "User Mismatch";
                    break;
                case AuthError.Failure:
                    validationText += "Failed to login...";
                    break;
                default:
                    validationText += "Issue in authentication: " + errorCode;
                    break;
            }
        }
        return validationText;
    }

    // clear all text fields
    public void ClearFields()
    {
        updateEmailInputField.text = "";
        updatePasswordInputField.text = "";
        updateUsernameInputField.text = "";
        updateEmailValidationText.text = "";
        updatePasswordValidationText.text = "";
        updateUsernameValidationText.text = "";
    }

    private IEnumerator ClosePanel(string messageText, TextMeshProUGUI panelText, GameObject panel, bool logOut)
    {
        panelText.color = Color.green;
        panelText.text = messageText;
        yield return new WaitForSeconds(1);
        panel.SetActive(false);
        DisplayProfile();

        if (logOut)
        {
            Logout();
        }
    }
}
