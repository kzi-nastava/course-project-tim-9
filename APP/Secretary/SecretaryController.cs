#nullable disable
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Net;
using System.Net.Mail;

[Route("api/[controller]")]
[ApiController]
public class SecretaryController : ControllerBase
{
    private IMongoDatabase _database;
    public SecretaryController()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb+srv://admin:admin@cluster0.ctjt6.mongodb.net/USI?retryWrites=true&w=majority");
        var client = new MongoClient(settings);
        _database = client.GetDatabase("USI");
    }

    // GET: api/Secretary/patients
    [HttpGet("patients")]
    public async Task<List<Patient>> GetUnblockedPatients()
    {
        var patients = _database.GetCollection<Patient>("Patients");

        return patients.Find(item => item.Active == "0").ToList();
    }

    // GET by Id: api/Secretary/patients/901
    [HttpGet("patients/{id}")]
    public async Task<Patient> GetUnblockedPatient(int id)
    {
        var patients = _database.GetCollection<Patient>("Patients");

        return patients.Find(item => item.Id == id && item.Active == "0").FirstOrDefault();
    }

    // GET: api/Secretary/patients/blocked
    [HttpGet("patients/blocked")]
    public async Task<List<Patient>> GetBlockedPatients()
    {
        var patients = _database.GetCollection<Patient>("Patients");

        return patients.Find(item => item.Active != "0").ToList();
    }

    // POST: api/Secretary/patients
    [HttpPost("patients")]
    public async Task<IActionResult> CreatePatient(int id, Patient patient)
    {
        var patients = _database.GetCollection<Patient>("Patients");

        if (patients.Find(item => item.Email == patient.Email).ToList().Count != 0)
        {
            return BadRequest("Error: email already exists!");
        }

        Random rnd = new Random();
        patient.Id = rnd.Next(901, 10000);

        // If patient with that id already exists generate another
        do
        {
            patient.Id = rnd.Next(901, 10000);
        }
        while (patients.Find(item => item.Id == id).ToList().Count != 0);

        patients.InsertOne(patient);

        return Ok();
    }

    // POST: api/Secretary/patients/901
    [HttpPut("patients/{id}")]
    public async Task<IActionResult> UpdatePatient(int id, Patient patient)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        Patient updatedPatient = patients.Find(p => p.Id == id).FirstOrDefault();

        updatedPatient.FirstName = patient.FirstName;
        updatedPatient.LastName = patient.LastName;
        updatedPatient.Email = patient.Email;
        updatedPatient.Password = patient.Password;
        updatedPatient.MedicalRecord.Weight = patient.MedicalRecord.Weight;
        updatedPatient.MedicalRecord.Height = patient.MedicalRecord.Height;
        updatedPatient.MedicalRecord.BloodType = patient.MedicalRecord.BloodType;

        patients.ReplaceOne(p => p.Id == id, updatedPatient);
        return Ok();
    }

    // DELETE: api/Secretary/patients/901

    [HttpDelete("patients/{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        patients.DeleteOne(p => p.Id == id);

        var examinations = _database.GetCollection<Examination>("MedicalExaminations");
        var filter = Builders<Examination>.Filter.Lt(e => e.DateAndTimeOfExamination, DateTime.Now.ToString("yyyy-MM-ddTHH:mm")) & Builders<Examination>.Filter.Eq("patient", id);
        examinations.DeleteMany(filter);

        return Ok();
    }

    [HttpPut("patients/block/{id}/{activityValue}")]
    // PUT: api/Secretary/patients/901/1
    public async Task<IActionResult> UpdatePatientActivity(int id, string activityValue)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        Patient updatedPatient = patients.Find(p => p.Id == id).FirstOrDefault();

        updatedPatient.Active = activityValue;

        patients.ReplaceOne(p => p.Id == id, updatedPatient);
        return Ok();
    }


    [HttpGet("patients/{id}/activity")]
    // GET: api/Secretary/patients/901/activity
    public async Task<String> GetPatientActivity(int id)
    {
        var patients = _database.GetCollection<Patient>("Patients"); ;

        return patients.Find(p => p.Id == id).FirstOrDefault().Active;
    }


    [HttpGet("doctors/speciality")]
    // GET: api/Secretary/doctors/speciality
    public async Task<List<String>> GetDoctorSpeciality(int id)
    {
        var collection = _database.GetCollection<Employee>("Employees");
        var doctors = collection.Find(d => d.Role == "doctor").ToList();

        List<string> allSpecializations = new List<string>();

        foreach (Employee e in doctors)
        {
            allSpecializations.Add(e.Specialization);
        }

        allSpecializations = allSpecializations.Distinct().ToList();

        return allSpecializations;
    }

    // GET: api/Secretary/examinationRequests
    [HttpGet("examinationRequests")]
    public async Task<List<ExaminationRequest>> GetExaminationRequests()
    {
        var requests = _database.GetCollection<ExaminationRequest>("ExaminationRequests");

        //Delete deprecated requests
        var filter = Builders<ExaminationRequest>.Filter.Lt(e => e.Examination.DateAndTimeOfExamination, DateTime.Now.ToString("yyyy-MM-ddTHH:mm"));
        requests.DeleteMany(filter);

        return requests.Find(item => true).ToList();
    }


    // GET: api/Secretary/examinations/100
    [HttpGet("examination/{id}")]
    public async Task<Examination> GetExamination(int id)
    {
        var examinations = _database.GetCollection<Examination>("MedicalExaminations");

        return examinations.Find(item => item.Id == id).FirstOrDefault();
    }

    // GET: api/Secretary/patients/100
    [HttpGet("patients/exists/{id}")]
    public async Task<bool> PatientExists(int id)
    {
        var patients = _database.GetCollection<Patient>("Patients");

        if (patients.Find(p => p.Id == id && p.Active == "0").CountDocuments() == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    // PUT: api/Secretary/examinationRequests/accept/1
    [HttpPut("examinationRequests/accept/{id}")]
    public async Task<IActionResult> AcceptExaminationRequest(string id)
    {
        var requests = _database.GetCollection<ExaminationRequest>("ExaminationRequests");
        ExaminationRequest examinationRequest = requests.Find(e => e._Id == id).FirstOrDefault();

        var examination = examinationRequest.Examination;


        var examinations = _database.GetCollection<Examination>("Examinations");

        if (examinationRequest.Status == 0)
        {
            examinations.DeleteOne(e => e.Id == examination.Id);
        }
        else
        {
            examinations.ReplaceOne(e => e.Id == examination.Id, examination);
        }
        requests.DeleteOne(e => e._Id == id);
        return Ok();
    }


    // PUT: api/Secretary/examinationRequests/decline/1
    [HttpPut("examinationRequests/decline/{id}")]
    public async Task<IActionResult> DeclineExaminationRequest(string id)
    {
        var requests = _database.GetCollection<ExaminationRequest>("ExaminationRequests");

        requests.DeleteOne(e => e._Id == id);

        return Ok();
    }

    public bool IsRoomOccupied(string examinationRoomName, string dateAndTimeOfExamination, int durationOfExamination)
    {
        var examinations = _database.GetCollection<Examination>("MedicalExaminations");
        var possiblyOccupiedRooms = examinations.Find(item => true).ToList();

        foreach (Examination item in possiblyOccupiedRooms)
        {
            if (item.RoomName == examinationRoomName)
            {
                DateTime itemBegin = DateTime.Parse(item.DateAndTimeOfExamination);
                DateTime itemEnd = itemBegin.AddMinutes(item.DurationOfExamination);
                DateTime examinationBegin = DateTime.Parse(dateAndTimeOfExamination);
                DateTime examinationEnd = examinationBegin.AddMinutes(durationOfExamination);
                if (examinationBegin >= itemBegin && examinationBegin <= itemEnd || examinationEnd >= itemBegin && examinationEnd <= itemEnd)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsRoomValid(string roomName)
    {
        var rooms = _database.GetCollection<Room>("Rooms");
        var resultingRoom = rooms.Find(r => r.Name == roomName && r.InRenovation == false);
        if (resultingRoom == null)
        {
            return false;
        }
        return true;
    }

    public bool IsValidPatient(int id)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        var resultingPatient = patients.Find(p => p.Id == id).FirstOrDefault();
        if (resultingPatient == null)
        {
            return false;
        }
        return true;
    }

    public bool IsDoctorFree(int doctorId, string examinationDate)
    {
        var doctorsExaminations = _database.GetCollection<Examination>("MedicalExamination").Find(e => e.DoctorId == doctorId).ToList();

        foreach (Examination e in doctorsExaminations)
        {
            DateTime doctorsExaminationBegin = DateTime.Parse(e.DateAndTimeOfExamination);
            DateTime doctorsExaminationEnd = doctorsExaminationBegin.AddMinutes(e.DurationOfExamination);
            DateTime examinationDateParsed = DateTime.Parse(examinationDate);
            if (doctorsExaminationBegin <= examinationDateParsed && doctorsExaminationEnd >= examinationDateParsed)
            {
                return false;
            }
        }
        return true;

    }

    public bool IsPatientFree(int patientId, string examinationDate)
    {
        var patientsExaminations = _database.GetCollection<Examination>("MedicalExamination").Find(e => e.PatinetId == patientId).ToList();

        foreach (Examination e in patientsExaminations)
        {
            DateTime patientsExaminationBegin = DateTime.Parse(e.DateAndTimeOfExamination);
            DateTime patientsExaminationEnd = patientsExaminationBegin.AddMinutes(e.DurationOfExamination);
            DateTime examinationDateParsed = DateTime.Parse(examinationDate);
            if (patientsExaminationBegin <= examinationDateParsed && patientsExaminationEnd >= examinationDateParsed)
            {
                return false;
            }
        }
        return true;

    }

    public bool IsRoomInRenovation(string doctorid, string examinationDate)
    {
        var renovations = _database.GetCollection<Renovation>("Renovations").Find(renovation => true).ToList();

        foreach (Renovation r in renovations)
        {
            DateTime renovationBegin = DateTime.Parse(r.StartDate);
            DateTime renovationEnd = DateTime.Parse(r.EndDate);
            DateTime examinationDateParsed = DateTime.Parse(examinationDate);
            if (renovationBegin <= examinationDateParsed && renovationEnd >= examinationDateParsed)
            {
                return true;
            }
        }
        return false;

    }

    public void DeletePatientReferral(int referralid, Examination newExamination)
    {
        var patients = _database.GetCollection<Patient>("Patients");
        Patient updatedPatient = patients.Find(p => p.Id == newExamination.PatinetId).FirstOrDefault();

        foreach (Referral patientReferral in updatedPatient.MedicalRecord.Referrals)
        {
            if (patientReferral.ReferralId == referralid)
            {
                updatedPatient.MedicalRecord.Referrals.Remove(patientReferral);
                break;
            }
        }

        patients.ReplaceOne(p => p.Id == newExamination.PatinetId, updatedPatient);
    }


    public bool CheckExaminationTimeValidity(Examination e)
    {
        var isValidPatient = IsValidPatient(e.PatinetId);
        var isValidRoom = IsRoomValid(e.RoomName);
        var isOccupiedRoom = IsRoomOccupied(e.RoomName, e.DateAndTimeOfExamination.ToString(), e.DurationOfExamination);
        var isRoomInRenovation = IsRoomInRenovation(e.RoomName, e.DateAndTimeOfExamination.ToString());
        var isDoctorFree = IsDoctorFree(e.DoctorId, e.DateAndTimeOfExamination.ToString());
        return isValidRoom && isValidPatient && !isRoomInRenovation && !isOccupiedRoom && isDoctorFree;
    }



    // GET: api/Secretary/examination/referral/create/none
    [HttpPost("examination/referral/create/{specialization}/{referralid}")]
    public async Task<IActionResult> CreateRefferedExamination(Examination newExamination, string specialization, int referralid)
    {
        var patients = _database.GetCollection<Patient>("Patients");

        if (specialization != "none")
        {
            var employees = _database.GetCollection<Employee>("Employees");
            List<Employee> specializedDoctors = employees.Find(e => e.Role == "doctor" && e.Specialization == specialization).ToList();
            if (specializedDoctors.Count() - 1 < 0)
            {

                DeletePatientReferral(referralid, newExamination);

                return BadRequest("Error: No such specialist exists");
            }
            Random rnd = new Random();
            newExamination.DoctorId = specializedDoctors[rnd.Next(0, specializedDoctors.Count() - 1)].Id;
        }

        if (newExamination.DurationOfExamination <= 15 || newExamination.DurationOfExamination >= 200)
        {
            return BadRequest();
        }

        var examinations = _database.GetCollection<Examination>("MedicalExaminations");

        var newExaminationDate = DateTime.Now.AddDays(1);


        DateTime upperDateTimelimit;
        DateTime lowerDateTimelimit;

        while (true)
        {
            newExamination.DateAndTimeOfExamination = newExaminationDate.ToString("yyyy-MM-ddTHH:mm");
            if (CheckExaminationTimeValidity(newExamination) && IsPatientFree(newExamination.PatinetId, newExamination.DateAndTimeOfExamination.ToString()))
            {
                upperDateTimelimit = new DateTime(newExaminationDate.Year, newExaminationDate.Month, newExaminationDate.Day, 8, 0, 0);
                lowerDateTimelimit = new DateTime(newExaminationDate.Year, newExaminationDate.Month, newExaminationDate.Day, 23, 59, 0);
                if (newExaminationDate >= lowerDateTimelimit && newExaminationDate <= upperDateTimelimit)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }

            else
            {
                newExaminationDate = newExaminationDate.AddMinutes(30);
            }

        }

        var rooms = _database.GetCollection<Room>("Rooms");
        var resultingRoom = rooms.Find(r => r.Name == newExamination.RoomName);

        if (resultingRoom == null)
        {
            return BadRequest();
        }
        var id = examinations.Find(e => true).SortByDescending(e => e.Id).FirstOrDefault().Id;
        newExamination.Id = id + 1;
        examinations.InsertOne(newExamination);

        DeletePatientReferral(referralid, newExamination);

        return Ok();

    }

    [HttpPost("examination/create/urgent/{specialization}")]
    public async Task<List<Examination>> CreateUrgentExamination(Examination newExamination, string specialization)
    {

        var examinations = _database.GetCollection<Examination>("MedicalExaminations");

        var patients = _database.GetCollection<Patient>("Patients");

        string roomType;
        if (newExamination.TypeOfExamination == "visit")
        {
            roomType = "examination room";
        }
        else
        {
            roomType = "operation room";
        }

        if (patients.Find(p => p.Id == newExamination.PatinetId).CountDocuments() == 0)
        {
            return new List<Examination>();
        }

        var room = _database.GetCollection<Room>("Rooms").Find(r => r.Type == roomType).FirstOrDefault();

        newExamination.RoomName = room.Name;

        var employees = _database.GetCollection<Employee>("Employees");
        List<Employee> specializedDoctors = employees.Find(e => e.Role == "doctor" && e.Specialization == specialization).ToList();

        var urgentExaminationDate = DateTime.Now;
        var urgentExaminationEnd = DateTime.Now.AddHours(2);


        while (urgentExaminationDate <= urgentExaminationEnd)
        {
            newExamination.DateAndTimeOfExamination = urgentExaminationDate.ToString("yyyy-MM-ddTHH:mm");
            foreach (Employee doctor in specializedDoctors)
            {
                newExamination.DoctorId = doctor.Id;

                if (CheckExaminationTimeValidity(newExamination))
                {
                    var rooms = _database.GetCollection<Room>("Rooms");
                    var resultingRoom = rooms.Find(r => r.Name == newExamination.RoomName);
                    var id = examinations.Find(e => true).SortByDescending(e => e.Id).FirstOrDefault().Id;
                    newExamination.Id = id + 1;
                    examinations.InsertOne(newExamination);
                    return new List<Examination>();
                }
            }
            urgentExaminationDate = urgentExaminationDate.AddMinutes(10);
        }

        var dateFilter = Builders<Examination>.Filter.Gt(e => e.DateAndTimeOfExamination, DateTime.Now.ToString("yyyy-MM-ddTHH:mm"));
        var roomFilter = Builders<Examination>.Filter.Eq(e => e.RoomName, newExamination.RoomName);
        var doctorFilter = Builders<Examination>.Filter.Eq(e => e.DoctorId, newExamination.DoctorId);

        var filter = dateFilter & roomFilter & doctorFilter;

        var examinationsAfterNow = examinations.Find(filter).SortBy(e => e.DateAndTimeOfExamination).ToList();

        List<Examination> fiveSortedExaminations = new List<Examination>();

        fiveSortedExaminations = examinationsAfterNow.Take(5).ToList();

        return fiveSortedExaminations;

    }


    public void SendTermNotificationEmailToPatient([FromRoute] Patient patient, [FromRoute] Employee employee, string oldDateAndTime, string newDateAndTime, int? examId)
    {
        var smptClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("teamnineMedical@gmail.com", "teamnine"),
            EnableSsl = true,
        };

        string messageDoctor = "Hello " + employee.FirstName + " " + employee.LastName
                    + "\n\n\nYour examination id:" + examId + " has been moved from " + oldDateAndTime + " to " +
                    newDateAndTime + ".\n\n\nPatient in question:\nid: " + patient.Id +
                    "\nName: " + patient.FirstName + "\nSurname: " + patient.LastName + "\n Have a nice day!";


        var mailMessageDoctor = new MailMessage
        {
            From = new MailAddress(employee.Email),
            Subject = "TeamNine Medical Team - IMPORTANT - examination moved",
            Body = messageDoctor,
            IsBodyHtml = true,
        };

        mailMessageDoctor.To.Add("teamnineMedical@gmail.com");
        smptClient.Send(mailMessageDoctor);
    }


    public void SendTermNotificationEmailToDoctor([FromRoute] Patient patient, [FromRoute] Employee employee, string oldDateAndTime, string newDateAndTime, int? examId)
    {
        var smptClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("teamnineMedical@gmail.com", "teamnine"),
            EnableSsl = true,
        };

        string messagePatient = "Hello " + patient.FirstName + " " + patient.LastName
                    + "\n\n\nYour examination id:" + examId + " has been moved from " + oldDateAndTime + " to " +
                    newDateAndTime + ".\n\n\nDoctor in question:" +
                    "\nName: " + employee.FirstName + "\nSurname: " + employee.LastName + "\n Have a nice day!";


        var mailMessagePatient = new MailMessage
        {
            From = new MailAddress(employee.Email),
            Subject = "TeamNine Medical Team - IMPORTANT - examination moved",
            Body = messagePatient,
            IsBodyHtml = true,
        };

        mailMessagePatient.To.Add("teamnineMedical@gmail.com");
        smptClient.Send(mailMessagePatient);
    }

    [HttpPost("examination/create/urgent")]
    public async Task<IActionResult> CreateUrgentExaminationWithTermMoving(Examination newExamination)
    {

        var examinations = _database.GetCollection<Examination>("MedicalExaminations");

        var reservedTimeFrames = examinations.Find(e => e.RoomName == newExamination.RoomName && e.DoctorId == newExamination.DoctorId).ToList();

        List<Examination> toMoveExaminations = new List<Examination>();

        var newExaminationBegin = DateTime.Parse(newExamination.DateAndTimeOfExamination);
        var newExaminationEnd = newExaminationBegin.AddMinutes(newExamination.DurationOfExamination);

        DateTime toMoveExamBegin;

        foreach (Examination e in reservedTimeFrames)
        {
            toMoveExamBegin = DateTime.Parse(e.DateAndTimeOfExamination);
            if (newExaminationBegin <= toMoveExamBegin && newExaminationEnd >= toMoveExamBegin)
            {
                toMoveExaminations.Add(e);
            }
        }


        var id = examinations.Find(e => true).SortByDescending(e => e.Id).FirstOrDefault().Id;
        newExamination.Id = id + 1;
        examinations.InsertOne(newExamination);

        var iterationDateTime = DateTime.Now;

        var patients = _database.GetCollection<Patient>("Patients");
        var employees = _database.GetCollection<Employee>("Employees");

        foreach (Examination toMoveExamination in toMoveExaminations)
        {
            var oldDateAndTime = toMoveExamination.DateAndTimeOfExamination;
            while (true)
            {
                toMoveExamination.DateAndTimeOfExamination = iterationDateTime.ToString("yyyy-MM-ddTHH:mm");
                if (CheckExaminationTimeValidity(toMoveExamination))
                {
                    Patient patient = patients.Find(p => p.Id == toMoveExamination.PatinetId).FirstOrDefault();
                    Employee employee = employees.Find(e => e.Id == toMoveExamination.DoctorId).FirstOrDefault();

                    SendTermNotificationEmailToPatient(patient, employee, oldDateAndTime, toMoveExamination.DateAndTimeOfExamination, toMoveExamination.Id);
                    SendTermNotificationEmailToDoctor(patient, employee, oldDateAndTime, toMoveExamination.DateAndTimeOfExamination, toMoveExamination.Id);

                    examinations.FindOneAndReplace(e => toMoveExamination.Id == e.Id, toMoveExamination);
                    break;
                }


                else
                {
                    iterationDateTime = iterationDateTime.AddMinutes(5);
                }
            }
        }


        return Ok();
    }


    // GET: api/Secretary/expendedDynamicEquipment
    [HttpGet("expendedDynamicEquipment")]
    public async Task<List<string>> GetExpendedDynamicEquipment()
    {
        var rooms = _database.GetCollection<Room>("Rooms");

        Dictionary<string, int> dynamicEquipmentQuantity = new Dictionary<string, int>();

        foreach (Room r in rooms.Find(item => true).ToList())
        {
            foreach (Equipment e in r.Equipment)
            {
                if (e.Type == "operation equipment")
                {
                    int oldQuantity;
                    if (dynamicEquipmentQuantity.TryGetValue(e.Name, out oldQuantity))
                    {
                        dynamicEquipmentQuantity[e.Name] = oldQuantity + e.Quantity;
                    }
                    else
                    {
                        dynamicEquipmentQuantity.Add(e.Name, e.Quantity);
                    }
                }
            }
        }

        List<string> expendedDynamicEquipment = new List<string>();

        foreach (KeyValuePair<string, int> equipmentQuantityEntry in dynamicEquipmentQuantity)
        {
            if (equipmentQuantityEntry.Value == 0)
            {
                expendedDynamicEquipment.Add(equipmentQuantityEntry.Key);
            }
        }

        return expendedDynamicEquipment;
    }


    // GET: api/Secretary/purchaseDynamicEquipment
    [HttpPost("purchaseDynamicEquipment")]
    public async Task<IActionResult> CreateDynamicEquipmentPurchase(Equipment purchasedEquipment)
    {
        Purchase newPurchase = new Purchase();
        newPurchase.Deadline = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm");
        newPurchase.Done = false;
        newPurchase.What.Add(purchasedEquipment);

        var purchases = _database.GetCollection<Purchase>("Purchases");
        purchases.InsertOne(newPurchase);

        return Ok();
    }

    // GET: api/Secretary/roomLowDynamicEquipment
    [HttpGet("roomLowDynamicEquipment")]
    public async Task<List<KeyValuePair<string, Equipment>>> GetRoomLowDynamicEquipment()
    {
        var rooms = _database.GetCollection<Room>("Rooms");

        List<KeyValuePair<string, Equipment>> lowDynamicEquipment = new List<KeyValuePair<string, Equipment>>();

        foreach (Room r in rooms.Find(item => item.Name != "Main warehouse").ToList())
        {
            foreach (Equipment e in r.Equipment)
            {
                if (e.Type == "operation equipment" && e.Quantity <= 5)
                {
                    lowDynamicEquipment.Add(new KeyValuePair<string, Equipment>(r.Name, e));
                }
            }
        }

        lowDynamicEquipment.Sort((x, y) => x.Value.Quantity.CompareTo(y.Value.Quantity));

        return lowDynamicEquipment;
    }

    // GET: api/Secretary/roomEquipmentQuantity/{roomName}/{equipmentName}
    [HttpGet("roomEquipmentQuantity/{roomName}/{equipmentName}")]
    public async Task<int> GetEquipmentQuantityRoom(string roomName, string equipmentName)
    {
        var rooms = _database.GetCollection<Room>("Rooms");

        var room = rooms.Find(r => r.Name == roomName).FirstOrDefault();

        foreach (Equipment roomEquipment in room.Equipment)
        {
            if (roomEquipment.Name == equipmentName)
            {
                return roomEquipment.Quantity;
            }
        }

        return 0;
    }


    [HttpPut("transferEquipment/{equipmentName}/{fromRoomName}/{toRoomName}/{quantity}")]
    public async Task<IActionResult> TransferDynamicEquipment(string equipmentName, string fromRoomName, string toRoomName, int quantity)
    {
        var rooms = _database.GetCollection<Room>("Rooms");

        var transferFromRoom = rooms.Find(r => r.Name == fromRoomName).FirstOrDefault();

        var transferToRoom = rooms.Find(r => r.Name == toRoomName).FirstOrDefault();

        foreach (Equipment fromRoomEquipment in transferFromRoom.Equipment)
        {
            if (fromRoomEquipment.Name == equipmentName)
            {
                if (fromRoomEquipment.Quantity - quantity < 0)
                {
                    return BadRequest();
                }
                else
                {
                    fromRoomEquipment.Quantity -= quantity;
                    rooms.ReplaceOne(r => r.Id == transferFromRoom.Id, transferFromRoom);
                }
            }
        }

        foreach (Equipment toRoomEquipment in transferToRoom.Equipment)
        {
            if (toRoomEquipment.Name == equipmentName)
            {
                toRoomEquipment.Quantity += quantity;
                rooms.ReplaceOne(r => r.Id == transferToRoom.Id, transferToRoom);
            }
        }

        return Ok();
    }

}