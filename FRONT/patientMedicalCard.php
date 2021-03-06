<!DOCTYPE html>
<html lang="en" dir="ltr">
    <head>
        <meta charset="utf-8">
        <meta name="description" content="USI medical institution team #9">
        <meta name="author" content="Tamara Ilic, Uros Pocek, Tamara Dzambic, Marko Erdelji">
        <meta name="keywords" content="usi">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">

        <link rel="icon" href="logo.jpeg">

        <link rel="stylesheet" type="text/css" href="style.css">
        <script src="https://kit.fontawesome.com/b4c27ec53d.js" crossorigin="anonymous"></script>

        <title>USI Team #9</title>

    </head>
    <body>
        
        <?php include 'header.html';?>


        <div id="examinationRefPopUp" class="form-container sign-in-container off prompt">
            <form id="examinationRefForm" class="colDir myForm">
                <h1 id="examinationRefFormId" >Create examination</h1>
                <div class="formDiv">
                    <label for="examinationType">Examination type:</label>
                    <select id="examinationRefType">
                        <option value="visit" selected>Visit</option>
                        <option value="operation">Operation</option>
                    </select>
                </div>
                <div class="formDiv">
                    <label for="examinationDuration">Duration:</label>
                    <input type="number" id="examinationRefDuration" min=15>
                </div>
                <div class="formDiv">
                    <label for="examinationRoom">Room:</label>
                    <select id="examinationRefRoom">
                        <!-- get rooms from api -->
                    </select>
                </div>
                <button class="mainBtn">OK</button>
            </form>
	    </div>
        
        <main id='medCardMain' class = 'myMain'>
            <section>
                <div id="patientInfo" >
                    <div class="basicInfo">
                        <h1>Medical record</h1>
                        <div>
                            <p>First name:&nbsp<span id="patientFName"></span></p>
                            <p>Last name:&nbsp<span id="patientLName"></span></p>
                            <p>Height:&nbsp<span id="patientHeight"></span></p>
                            <p>Weight:&nbsp<span id="patientWeight"></span></p>
                            <p>Blood type:&nbsp<span id="patientBlood"></span></p>
                            <div class="divList">
                                <p>Diseases:&nbsp</p>
                                <ul id="diseasesList">
                                    <!-- patients diseases -->
                                </ul>
                            </div>
                            <div class="divList">
                                <p>Alergies:&nbsp</p>
                                <ul id="alergiesList">
                                    <!-- patients diseases -->
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
            
            <section class="addSpaceBottom">
                <div id="rooms">
                    <div class="tbl-content">
                        <table cellpadding="0" cellspacing="0" border="0">
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    <th>Duration</th>
                                    <th>Done</th>
                                    <th>Examination room</th>
                                    <th>Type</th>
                                    <th>Urgent</th>
                                </tr>
                            </thead>
                            <tbody id="examinationsTable">
                                <!-- this is where data from api comes -->
                            </tbody>
                        </table>
                    </div>
                </div>
            </section>

            <section class="addSpaceBottom">
                <div id="rooms" class="instructions">
                    <div class="tbl-content">
                        <table cellpadding="0" cellspacing="0" border="0">
                            <thead>
                                <tr>
                                    <th>Start date</th>
                                    <th>End date</th>
                                    <th>Drug</th>
                                    <th>Doctor</th>
                                </tr>
                            </thead>
                            <tbody id="instructionsTable">
                                <!-- this is where data from api comes -->
                            </tbody>
                        </table>
                    </div>
                </div>
            </section>

            <section id="referralSection" class="addSpaceBottom off">
                <div id="rooms" class="referrals">
                    <div class="tbl-content">
                        <table cellpadding="0" cellspacing="0" border="0">
                            <thead>
                                <tr>
                                    <th>Doctor</th>
                                    <th>Specialization</th>
                                </tr>
                            </thead>
                            <tbody id="referralsTable">
                                <!-- this is where data from api comes -->
                            </tbody>
                        </table>
                    </div>
                </div>
            </section>
                
        </main>

        <script src="medical_card.js"></script>
    </body>
</html>