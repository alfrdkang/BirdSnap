using System;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Google.MiniJSON;
using TMPro;
using UnityEditor;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Firebase;
using Firebase.Extensions;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Object = System.Object;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    
    private DatabaseReference reference;
    private FirebaseAuth auth;
    private FirebaseUser user;

    public int numberOfPlayers;
    
    [SerializeField] private GameObject homeScreen;
    [SerializeField] private GameObject signupScreen;
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject resetPasswordScreen;
    
    //Signup
    [SerializeField] private TMP_InputField signupUsernameInputField;
    [SerializeField] private TMP_InputField signupEmailInputField;
    [SerializeField] private TMP_InputField signupPasswordInputField;
    [SerializeField] private TMP_InputField signupConfirmPasswordInputField;
    [SerializeField] private TextMeshProUGUI signupValidationText;
    
    //Login
    [SerializeField] private TMP_InputField loginEmailInputField;
    [SerializeField] private TMP_InputField loginPasswordInputField;
    [SerializeField] private TextMeshProUGUI loginValidationText;
    
    //Reset Password
    [SerializeField] private TMP_InputField resetPasswordEmailInputField;
    [SerializeField] private TextMeshProUGUI resetPasswordValidationText;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        
        
        if (auth.CurrentUser != null)
        {
            //update last login
            Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
            childUpdates["players/" + auth.CurrentUser.UserId + "/lastLoginDate/"] = ConvertNowToTimeStamp();

            reference.UpdateChildrenAsync(childUpdates);
            
            loginScreen.SetActive(false);
            homeScreen.SetActive(true);
            ResetFields();
            
            // auth.SignOut(); //NOTE: remove on build, add auto switch to homescreen if signed in
        }

        AuthStateChanged(this, null);
        reference.Child("players").ValueChanged += HandlePlayerValueChanged;
    }
    
    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
        if (auth.CurrentUser != user) {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn) {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    // Handle removing subscription and reference to the Auth instance.
    void OnDestroy() {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void HandlePlayerValueChanged(object send, ValueChangedEventArgs
        args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        numberOfPlayers = (int)args.Snapshot.ChildrenCount;
        //playerCountText.text = numberOfPlayers.ToString();
    }
    
    //Account Interaction Functions

    public void SignUp()  
    {
        Debug.Log("Submit Values (Signup)");
        if (!string.IsNullOrWhiteSpace(signupUsernameInputField.text) && 
            !string.IsNullOrWhiteSpace(signupEmailInputField.text) && 
            !string.IsNullOrWhiteSpace(signupPasswordInputField.text) && 
            !string.IsNullOrWhiteSpace(signupConfirmPasswordInputField.text))
        {
            if (signupPasswordInputField.text == signupConfirmPasswordInputField.text)
            {
                string username = signupUsernameInputField.text;
                string email = signupEmailInputField.text;
                string password = signupPasswordInputField.text;
                
                auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    signupValidationText.text = HandleAuthExceptions(task.Exception);

                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    signupScreen.SetActive(false);
                    homeScreen.SetActive(true);
                    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
                        if (task.IsCanceled) {
                            Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted)
                        {
                            Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                            string errortext = HandleAuthExceptions(task.Exception).ToString();
                            return;
                        }
                        Firebase.Auth.AuthResult result = task.Result;
                        Debug.LogFormat("User signed in successfully: {0} ({1})",
                            result.User.DisplayName, result.User.UserId);
                        WriteNewPlayer(
                            result.User.UserId,
                            username,
                            email,
                            ConvertNowToTimeStamp(),
                            ConvertNowToTimeStamp(),
                            ConvertNowToTimeStamp(),
                            0,
                            0,
                            0,
                            0, 
                            new string[] { "New Snapper" },
                            0f
                        );
                        ResetFields();
                    });
                }

                // Firebase user has been created.
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("auth user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
                });
            }
            else
            {
                signupValidationText.text = "Passwords do not match";
            }
        }
        else
        {
            signupValidationText.text = "Please fill all fields";
        }
    }

    public void Login()
    {
        Debug.Log("Submit Values (Login)");
        if (!string.IsNullOrWhiteSpace(loginEmailInputField.text) &&
            !string.IsNullOrWhiteSpace(loginPasswordInputField.text))
        {
            auth.SignInWithEmailAndPasswordAsync(loginEmailInputField.text, loginPasswordInputField.text)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        loginValidationText.text = HandleAuthExceptions(task.Exception);
                        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    if (task.IsCompleted)
                    {
                        loginScreen.SetActive(false);
                        homeScreen.SetActive(true);
                        ResetFields();
                    }

                    Firebase.Auth.AuthResult result = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                });
        }
        else
        {
            loginValidationText.text = "Please fill all fields";
        }
    }

    public void ResetPassword()
    {
        if (!string.IsNullOrWhiteSpace(resetPasswordEmailInputField.text))
        {
            auth.SendPasswordResetEmailAsync(resetPasswordEmailInputField.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    resetPasswordValidationText.text = HandleAuthExceptions(task.Exception);
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                if (task.IsCompleted)
                {
                    loginScreen.SetActive(true);
                    resetPasswordScreen.SetActive(false);
                    ResetFields();
                }

                Debug.Log("Password reset email sent successfully.");
            });
        }
        else
        {
            resetPasswordValidationText.text = "Please fill all fields";
        }
    }

    public void ResetFields()
    {
        signupUsernameInputField.text = "";
        signupEmailInputField.text = "";
        signupPasswordInputField.text = "";
        signupConfirmPasswordInputField.text = "";
        signupValidationText.text = "";
        loginEmailInputField.text = "";
        loginPasswordInputField.text = "";
        loginValidationText.text = "";
        resetPasswordValidationText.text = "";
        resetPasswordEmailInputField.text = "";
    }
    
    
    //Account Validation Functions
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
    
    //Db Writing Functions
    private void WriteNewPlayer(string uid, string name, string email, string creationDate, string lastLoginDate, string updatedDate, int highScore, int gamesPlayed, int birdsSnapped, int totalSnaps, string[] achievements, float accuracy)
    {
        PlayerData player = new PlayerData(name, email, creationDate, lastLoginDate, highScore, gamesPlayed, birdsSnapped, totalSnaps, achievements, accuracy);
        Leaderboard leaderboardEntry = new Leaderboard(name, highScore, birdsSnapped, accuracy, updatedDate);
        
        string playerJson = JsonUtility.ToJson(player);
        string leaderboardJson = JsonUtility.ToJson(leaderboardEntry);
        Debug.Log(playerJson);
        Debug.Log(leaderboardJson);
        reference.Child("players").Child(uid).SetRawJsonValueAsync(playerJson);
        reference.Child("leaderboard").Child(uid).SetRawJsonValueAsync(leaderboardJson);
    }

    public void UpdatePlayData(int score, float birdsSnapped, float totalSnaps)
    {
        reference.Child("players").Child(user.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("UpdatePlayData GetValueAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("UpdatePlayData GetValueAsync encountered an error: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int highScore = int.Parse(snapshot.Child("highScore").Value.ToString());
                int prevBirdsSnapped = int.Parse(snapshot.Child("birdsSnapped").Value.ToString());
                int prevTotalSnaps = int.Parse(snapshot.Child("totalSnaps").Value.ToString());
                int gamesPlayed = int.Parse(snapshot.Child("gamesPlayed").Value.ToString());
                
                Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
                
                childUpdates["players/" + user.UserId + "/gamesPlayed/"] = gamesPlayed+1;
                childUpdates["players/" + user.UserId + "/birdsSnapped/"] = prevBirdsSnapped + birdsSnapped;
                childUpdates["players/" + user.UserId + "/totalSnaps/"] = prevTotalSnaps + totalSnaps;
                childUpdates["players/" + user.UserId + "/accuracy/"] = Math.Round(((prevBirdsSnapped + birdsSnapped) / (prevTotalSnaps + totalSnaps) * 100),2);
                
                childUpdates["leaderboard/" + user.UserId + "/birdsSnapped/"] = prevBirdsSnapped + birdsSnapped;
                childUpdates["leaderboard/" + user.UserId + "/accuracy/"] = Math.Round(((prevBirdsSnapped + birdsSnapped) / (prevTotalSnaps + totalSnaps) * 100),2);
                childUpdates["leaderboard/" + user.UserId + "/updatedDate"] = ConvertNowToTimeStamp();
                
                if (score > highScore)
                {
                    childUpdates["players/" + user.UserId + "/highScore/"] = score;
                    childUpdates["leaderboard/" + user.UserId + "/highScore/"] = score;
                }

                reference.UpdateChildrenAsync(childUpdates);
            }
        });
    }
    
    public string ConvertNowToTimeStamp()
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        // Get the unix timestamp in seconds
        return dto.ToUnixTimeSeconds().ToString();
    }
}
