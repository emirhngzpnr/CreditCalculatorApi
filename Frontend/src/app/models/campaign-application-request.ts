export interface CampaignRequest{
 creditType: number;       // enum'ı sayısal olarak gönderiyoruz
  minVade: number;
  maxVade: number;
  minKrediTutar: number;
  maxKrediTutar: number;
  baslangicTarihi: string;  // "2025-08-01T00:00:00"
  bitisTarihi: string;
  description: string;
  faizOrani: number;
  bankId: number;
}