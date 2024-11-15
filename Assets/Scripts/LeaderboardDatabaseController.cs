using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using Object = System.Object;

public class LeaderboardDatabaseController : MonoBehaviour
{
    private DatabaseReference reference;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private int sortIndex = 0;
    
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    
    [SerializeField] private Sprite sortSpriteInactive;
    [SerializeField] private Sprite sortSpriteActive;
    [SerializeField] private Sprite userLeaderboardBgSprite;
    [SerializeField] private Image hScoreSortImg;
    [SerializeField] private Image birdsSnappedSortImg;
    [SerializeField] private Image accuracySortImg;
    
    [SerializeField] private TextMeshProUGUI profileUsernameText;
    [SerializeField] private TextMeshProUGUI profileHighscoreText;
    [SerializeField] private TextMeshProUGUI profileBirdsSnappedText;
    [SerializeField] private TextMeshProUGUI profileAccuracyText;
    
    public void DisplayLeaderboard()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        
        //Display Leaderboard Profile
        if (user != null)
        {
            reference.Child("leaderboard").Child(user.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
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
                    string highScore = snapshot.Child("highScore").Value.ToString();
                    string birdsSnapped = snapshot.Child("birdsSnapped").Value.ToString();
                    string accuracy = snapshot.Child("accuracy").Value.ToString();
                
                    profileUsernameText.text = username;
                    profileHighscoreText.text = "High Score: " + highScore;
                    profileBirdsSnappedText.text = "Birds Snapped: " + birdsSnapped;
                    profileAccuracyText.text = "Accuracy: " + accuracy + "%";

                }
            });
        }

        if (sortIndex == 0) // Highscore Sort
        {
            hScoreSortImg.sprite = sortSpriteActive;
            birdsSnappedSortImg.sprite = sortSpriteInactive;
            accuracySortImg.sprite = sortSpriteInactive;
            
            FirebaseDatabase.DefaultInstance
            .GetReference("leaderboard").OrderByChild("highScore").LimitToLast(30)
            .ValueChanged += HandleValueChanged;

            void HandleValueChanged(object sender, ValueChangedEventArgs args) {
                if (args.DatabaseError != null) {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                } 
                if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
                {
                    //reset existing leaderboard child
                    foreach (Transform child in leaderboardPanel.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    int i = 0;
                    List<DataSnapshot> childSnapshots = new List<DataSnapshot>(args.Snapshot.Children);
                    childSnapshots.Reverse();
                    
                    //instantiate leaderboard entries
                    foreach (var childSnapshot in args.Snapshot.Children)
                    {
                        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardPanel.transform);
                        
                        //check if user
                        if (childSnapshots[i].Key == auth.CurrentUser.UserId)
                        {
                            leaderboardEntry.GetComponent<Image>().sprite = userLeaderboardBgSprite;
                        }
                        
                        //special colours for top 3
                        if ((i + 1) == 1)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.yellow;
                        } else if ((i + 1) == 2)
                        {   
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
                        } else if ((i + 1) == 3)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = new Color32(145, 85, 77,255);
                        }
                        
                        leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "#" + (i+1);
                        //check if user
                        if (childSnapshots[i].Key == user.UserId)
                        {
                            leaderboardEntry.GetComponent<Image>().sprite = userLeaderboardBgSprite;
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "(YOU) " +
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        else
                        {
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        leaderboardEntry.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "High Score: " + childSnapshots[i].Child("highScore").Value;
                        i++;
                    }
                }
            }
        } 
        else if (sortIndex == 1) // Birds Snapped Sort
        {
            hScoreSortImg.sprite = sortSpriteInactive;
            birdsSnappedSortImg.sprite = sortSpriteActive;
            accuracySortImg.sprite = sortSpriteInactive;
            
            FirebaseDatabase.DefaultInstance
            .GetReference("leaderboard").OrderByChild("birdsSnapped").LimitToLast(30)
            .ValueChanged += HandleValueChanged;

            void HandleValueChanged(object sender, ValueChangedEventArgs args) {
                if (args.DatabaseError != null) {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                } 
                if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
                {
                    //reset existing leaderboard child
                    foreach (Transform child in leaderboardPanel.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    int i = 0;
                    List<DataSnapshot> childSnapshots = new List<DataSnapshot>(args.Snapshot.Children);
                    childSnapshots.Reverse();
                    
                    //instantiate leaderboard entries
                    foreach (var childSnapshot in args.Snapshot.Children)
                    {
                        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardPanel.transform);
                        
                        //check if user
                        if (childSnapshots[i].Key == user.UserId)
                        {
                            leaderboardEntry.GetComponent<Image>().sprite = userLeaderboardBgSprite;
                        }
                        
                        //special colours for top 3
                        if ((i + 1) == 1)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.yellow;
                        } else if ((i + 1) == 2)
                        {   
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
                        } else if ((i + 1) == 3)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = new Color32(145, 85, 77,255);
                        }
                        
                        leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "#" + (i+1);
                        //check if user
                        if (childSnapshots[i].Key == user.UserId)
                        {
                            leaderboardEntry.GetComponent<Image>().sprite = userLeaderboardBgSprite;
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "(YOU) " +
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        else
                        {
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        leaderboardEntry.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "Birds Snapped: " + childSnapshots[i].Child("birdsSnapped").Value;
                        i++;
                    }
                }
            }
        } else if (sortIndex == 2) // Accuracy Sort
        {
            hScoreSortImg.sprite = sortSpriteInactive;
            birdsSnappedSortImg.sprite = sortSpriteInactive;
            accuracySortImg.sprite = sortSpriteActive;
            
            FirebaseDatabase.DefaultInstance
            .GetReference("leaderboard").OrderByChild("accuracy").LimitToLast(30)
            .ValueChanged += HandleValueChanged;

            void HandleValueChanged(object sender, ValueChangedEventArgs args) {
                if (args.DatabaseError != null) {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                } 
                if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
                {
                    //reset existing leaderboard child
                    foreach (Transform child in leaderboardPanel.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    int i = 0;
                    List<DataSnapshot> childSnapshots = new List<DataSnapshot>(args.Snapshot.Children);
                    childSnapshots.Reverse();
                    
                    //instantiate leaderboard entries
                    foreach (var childSnapshot in args.Snapshot.Children)
                    {
                        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardPanel.transform);
                        
                        //special colours for top 3
                        if ((i + 1) == 1)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.yellow;
                        } else if ((i + 1) == 2)
                        {   
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
                        } else if ((i + 1) == 3)
                        {
                            leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = new Color32(145, 85, 77,255);
                        }
                        
                        leaderboardEntry.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "#" + (i+1);
                        //check if user
                        if (childSnapshots[i].Key == user.UserId)
                        {
                            leaderboardEntry.GetComponent<Image>().sprite = userLeaderboardBgSprite;
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "(YOU) " +
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        else
                        {
                            leaderboardEntry.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                                childSnapshots[i].Child("name").Value.ToString();
                        }
                        leaderboardEntry.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text =
                            "Accuracy: " + childSnapshots[i].Child("accuracy").Value+"%";
                        i++;
                    }
                }
            }
        }
    }

    public void LeaderboardSortIndex(int index)
    {
        sortIndex = index;
        DisplayLeaderboard();
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
}
