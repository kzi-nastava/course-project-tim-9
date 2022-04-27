#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Cors;


namespace APP.Controllers{
[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private IMongoDatabase database;
    public PatientController()
    {
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://admin:admin@cluster0.ctjt6.mongodb.net/USI?retryWrites=true&w=majority");
            var client = new MongoClient(settings);
            database = client.GetDatabase("USI");
    }

    // GET action

    // GET: api/Patient/doctors
    [HttpGet("doctors")]

    public IActionResult GetAllDoctors(){
            var collection = database.GetCollection<BsonDocument>("Employees");

            var filter = Builders<BsonDocument>.Filter.Eq("role", "doctor");
            var results = collection.Find(filter).ToList();
            var dotNetObjList = results.ConvertAll(BsonTypeMapper.MapToDotNetValue);
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(dotNetObjList);
    }


    // GET by Id action

    // GET: api/Patient/examinations/id
    [HttpGet("examinations/{id}")]

    public async  Task<List<Examination>> GetPatientsExaminations(int id){
          var examinationCollection = database.GetCollection<Examination>("MedicalExaminations");
        return examinationCollection.Find(e => e.patinetId == id).ToList();
  }


    // POST action

    [HttpPost("examinations")]
    public async Task<IActionResult> CreateExamination(Examination examination)
    {
        var patients = database.GetCollection<Patient>("Patients");
        var patient = patients.Find(p => p.id == examination.patinetId).FirstOrDefault();


        var examinations = database.GetCollection<Examination>("MedicalExaminations");

        var doctorsExaminations = examinations.Find(item => item.doctorId == examination.doctorId).ToList();
        foreach (var item in doctorsExaminations){
                DateTime itemBegin = DateTime.Parse(item.dateAndTimeOfExamination);
                DateTime itemEnd = itemBegin.AddMinutes(item.durationOfExamination);
                DateTime examinationBegin = DateTime.Parse(examination.dateAndTimeOfExamination);
                DateTime examinationEnd = examinationBegin.AddMinutes(examination.durationOfExamination);
                if(examinationBegin > itemBegin && examinationBegin < itemEnd || examinationEnd > itemBegin && examinationEnd < itemEnd){
                        return BadRequest();
                }
        }        

        var rooms = database.GetCollection<Room>("Rooms");
        var validRooms = rooms.Find(room => room.inRenovation == false && room.type == "examination room").ToList();
        
        foreach (var room  in validRooms)
        {
            var examinationsInRoom = examinations.Find(item => item.roomName == room.name && item.dateAndTimeOfExamination != examination.dateAndTimeOfExamination).ToList();
            if(examinationsInRoom != null){
                examination.roomName = room.name;
                break;
            }
             
        }


        var id = examinations.Find(e => true).SortByDescending(e => e.id).FirstOrDefault().id;
        examination.id = id + 1;
        examinations.InsertOne(examination);

        ExaminationHistoryEntry newEntry = new ExaminationHistoryEntry();
        newEntry.date = DateTime.Today.ToString();
        newEntry.type = "created";
        patient.examinationHistory.Add(newEntry);

        return Ok();       
    }

    [HttpPost("examinationRequests")]
    public async Task<IActionResult> createRequest(ExaminationRequest request)
    {
        var requests = database.GetCollection<ExaminationRequest>("ExaminationRequests");
        requests.InsertOne(request);
        return Ok(); 
    }


    // PUT action
    [HttpPut("examinations/{id}")]
        public async Task<IActionResult> UpdateExamination(string id,[FromBody] Examination examination)
    {
        var patients = database.GetCollection<Patient>("Patients");
        var patient = patients.Find(p => p.id == examination.patinetId).FirstOrDefault();

       
        var examinations = database.GetCollection<Examination>("MedicalExaminations");
        var oldExaminationData = examinations.Find(item => item.id == int.Parse(id)).FirstOrDefault();
        examination._id = oldExaminationData._id;
        examination.id = oldExaminationData.id;

        var doctorsExaminations = examinations.Find(item => item.doctorId == examination.doctorId).ToList();
        foreach (var item in doctorsExaminations){
                DateTime itemBegin = DateTime.Parse(item.dateAndTimeOfExamination);
                DateTime itemEnd = itemBegin.AddMinutes(item.durationOfExamination);
                DateTime examinationBegin = DateTime.Parse(examination.dateAndTimeOfExamination);
                DateTime examinationEnd = examinationBegin.AddMinutes(examination.durationOfExamination);
                if(examinationBegin > itemBegin && examinationBegin < itemEnd || examinationEnd > itemBegin && examinationEnd < itemEnd){
                        return BadRequest();
                }
        }        

        var rooms = database.GetCollection<Room>("Rooms");
        var validRooms = rooms.Find(room => room.inRenovation == false && room.type == "examination room").ToList();

        foreach (var room  in validRooms)
        {
            var examinationsInRoom = examinations.Find(item => item.roomName == room.name && item.dateAndTimeOfExamination != examination.dateAndTimeOfExamination).ToList();
            if(examinationsInRoom != null){
                examination.roomName = room.name;
                break;
            }
             
        }


        DateTime dt = DateTime.Today;
        DateTime dtOfExamination = DateTime.Parse(oldExaminationData.dateAndTimeOfExamination);
        
        if(dt.AddDays(2) >= dtOfExamination){
            ExaminationRequest request = new ExaminationRequest();
            request.examination = examination;
            request.status = 1;
            await createRequest(request);
            return Ok();
        } 
        examinations.FindOneAndReplace(e => e.id == int.Parse(id), examination);
        return Ok();
        

     
    }

    // DELETE action
    [HttpDelete("examinations/{id}")]
        public async Task<IActionResult> DeleteExamination(string id)
        {
            var examinations = database.GetCollection<Examination>("MedicalExaminations");
            Examination examination = examinations.Find(item => item.id == int.Parse(id)).FirstOrDefault();
            var patients = database.GetCollection<Patient>("Patients");
            
            DateTime dt = DateTime.Today;
            DateTime dtOfExamination = DateTime.Parse(examination.dateAndTimeOfExamination);

        if(dt.AddDays(2) >= dtOfExamination){
            ExaminationRequest request = new ExaminationRequest();
            request.examination = examination;
            request.status = 0;
            await createRequest(request);
            return Ok();
        }
                
            examinations.DeleteOne(item => item.id == int.Parse(id));
            return Ok();

            //inace posalji zahtev
            
            //izbrisi iz liste pregleda kod pacijenta i dodaj u istoriju izmena
           
        }




}
}