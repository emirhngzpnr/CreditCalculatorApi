export interface CampaignResponse{
    id: number;
  creditType: string;        // Enum'ın string hali geliyor
  minVade: number;
  maxVade: number;
  minKrediTutar: number;
  maxKrediTutar: number;
  recordTime: string;        // ISO string (backend'den DateTime geldiği için)
  baslangicTarihi: string;
  bitisTarihi: string;
  description: string;
  faizOrani: number;         
  bankId: number;
  bankName: string;
}