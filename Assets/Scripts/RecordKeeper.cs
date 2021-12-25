using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordKeeper : MonoBehaviour
{

    public static RecordKeeper Instance;

    List<Theif> theives = new List<Theif>();
    List<Police> polices = new List<Police>();
    List<Healer> healers = new List<Healer>();
    List<Worker> workers = new List<Worker>();
    List<Assassin> assassins= new List<Assassin>();
    List<Miner> miners = new List<Miner>();
    List<Investor> investors = new List<Investor>();

    List<House> houses = new List<House>();
    List<Mine> mines = new List<Mine>();
    List<Prison> prisons = new List<Prison>();
    List<IdlePoint> idlePoints = new List<IdlePoint>();

    List<GameObject> NPCs = new List<GameObject>();
    List<GameObject> buildings = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void AddEntity(GameObject entity)
    {
        if (entity.GetComponent<Person>())
        {
            if (!NPCs.Contains(entity))
                NPCs.Add(entity);

            if (entity.GetComponent<Theif>())
                addTheif(entity);
            else if (entity.GetComponent<Police>())
                addPolice(entity);
            else if (entity.GetComponent<Healer>())
                addHealer(entity);
            else if (entity.GetComponent<Worker>())
                addWorker(entity);
            else if (entity.GetComponent<Assassin>())
                addAssassin(entity);
            else if (entity.GetComponent<Miner>())
                addMiner(entity);
            else if (entity.GetComponent<Investor>())
                addInvestor(entity);
        }
        else if (entity.GetComponent<Building>())
        {
            if (!buildings.Contains(entity))
                buildings.Add(entity);

            if (entity.GetComponent<House>())
                addHouse(entity);
            else if (entity.GetComponent<Mine>())
                addMine(entity);
            else if (entity.GetComponent<Prison>())
                addPrison(entity);

        }else if (entity.GetComponent<IdlePoint>() && !idlePoints.Contains(entity.GetComponent<IdlePoint>()))
        {
            idlePoints.Add(entity.GetComponent<IdlePoint>());
        }
    }
    #region Add (Insert name here) Methods
    //Seperated into different methods to allow for future changes (if needed) depending on the type of the thing being added
    private void addTheif(GameObject entity)
    {
        Theif entityTheifScript = entity.GetComponent<Theif>();
        if (!theives.Contains(entityTheifScript))
            theives.Add(entityTheifScript);
    }
    private void addPolice(GameObject entity)
    {
        Police entityPoliceScript = entity.GetComponent<Police>();
        if (!polices.Contains(entityPoliceScript))
            polices.Add(entityPoliceScript);
    }
    private void addHealer(GameObject entity)
    {
        Healer entityHealerScript = entity.GetComponent<Healer>();
        if (!healers.Contains(entityHealerScript))
            healers.Add(entityHealerScript);
    }
    private void addWorker(GameObject entity)
    {
        Worker entityWorkerScript = entity.GetComponent<Worker>();
        if (!workers.Contains(entityWorkerScript))
            workers.Add(entityWorkerScript);
    }
    private void addAssassin(GameObject entity)
    {
        Assassin entityAssassinScript = entity.GetComponent<Assassin>();
        if (!assassins.Contains(entityAssassinScript))
            assassins.Add(entityAssassinScript);
    }
    private void addMiner(GameObject entity)
    {
        Miner entityMinerScript = entity.GetComponent<Miner>();
        if (!miners.Contains(entityMinerScript))
            miners.Add(entityMinerScript);
    }
    private void addInvestor(GameObject entity)
    {
        Investor entityInvestorScript = entity.GetComponent<Investor>();
        if (!investors.Contains(entityInvestorScript))
            investors.Add(entityInvestorScript);
    }
    private void addMine(GameObject entity)
    {
        Mine entityMineScript = entity.GetComponent<Mine>();
        if (!mines.Contains(entityMineScript))
            mines.Add(entityMineScript);
    }

    private void addHouse(GameObject entity)
    {
        House entityHouseScript = entity.GetComponent<House>();
        if (!houses.Contains(entityHouseScript))
            houses.Add(entityHouseScript);
    }
    private void addPrison(GameObject entity)
    {
        Prison entityPrisonScript = entity.GetComponent<Prison>();
        if (!prisons.Contains(entityPrisonScript))
            prisons.Add(entityPrisonScript);
    }
    #endregion
    public void RemoveEntity(GameObject entity)
    {
        if (NPCs.Contains(entity))
        {
            NPCs.Remove(entity);

            if (entity.GetComponent<Theif>() && theives.Contains(entity.GetComponent<Theif>()))
                theives.Remove(entity.GetComponent<Theif>());
            else if (entity.GetComponent<Police>() && polices.Contains(entity.GetComponent<Police>()))
                polices.Remove(entity.GetComponent<Police>());
            else if (entity.GetComponent<Healer>() && healers.Contains(entity.GetComponent<Healer>()))
                healers.Remove(entity.GetComponent<Healer>());
            else if (entity.GetComponent<Worker>() && workers.Contains(entity.GetComponent<Worker>()))
                workers.Remove(entity.GetComponent<Worker>());
            else if (entity.GetComponent<Assassin>() && assassins.Contains(entity.GetComponent<Assassin>()))
                assassins.Remove(entity.GetComponent<Assassin>());
            else if (entity.GetComponent<Miner>() && miners.Contains(entity.GetComponent<Miner>()))
                miners.Remove(entity.GetComponent<Miner>());
            else if (entity.GetComponent<Investor>() && investors.Contains(entity.GetComponent<Investor>()))
                investors.Remove(entity.GetComponent<Investor>());
        }
        else if (buildings.Contains(entity))
        {
            buildings.Remove(entity);
            if (entity.GetComponent<House>() && houses.Contains(entity.GetComponent<House>()))
                houses.Remove(entity.GetComponent<House>());
            else if (entity.GetComponent<Mine>() && mines.Contains(entity.GetComponent<Mine>()))
                mines.Remove(entity.GetComponent<Mine>());
            else if (entity.GetComponent<Prison>() && prisons.Contains(entity.GetComponent<Prison>()))
                prisons.Remove(entity.GetComponent<Prison>());
        }
        else if (idlePoints.Contains(entity.GetComponent<IdlePoint>()))
        {
            idlePoints.Add(entity.GetComponent<IdlePoint>());
        }
    }
    #region Remove Gameobject Methods
    //Not Implemented (Yet?)
    #endregion

    #region List Gets
    public List<Theif> GetTheives()
    {
        return new List<Theif>(theives);
    }
    public List<Police> GetPolices()
    {
        return new List<Police>(polices);
    }
    public List <Healer> GetHealers()
    {
        return new List<Healer>(healers);
    }
    public List<Assassin> GetAssassins()
    {
        return new List<Assassin>(assassins);
    }
    public List<Worker> GetWorkers()
    {
        return new List<Worker>(workers);
    }
    public List<Miner> GetMiners()
    {
        return new List<Miner>(miners);
    }
    public List<Investor> GetInvestors()
    {
        return new List<Investor>(investors);
    }
    public List<House> GetHouses()
    {
        return new List<House>(houses);
    }
    public List<Mine> GetMines()
    {
        return new List<Mine>(mines);
    }
    public List<Prison> GetPrisons()
    {
        return new List<Prison>(prisons);
    }
    public List<IdlePoint> GetIdlePoints()
    {
        return new List<IdlePoint>(idlePoints);
    }
    public List<GameObject> GetNPCs()
    {
        return new List<GameObject>(NPCs);
    }
    public List<GameObject> GetBuildings()
    {
        return new List<GameObject>(buildings);
    }
    #endregion
}
